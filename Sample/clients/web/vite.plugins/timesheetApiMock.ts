import fs from 'node:fs'
import type { ServerResponse } from 'node:http'
import type { Connect, Plugin } from 'vite'

type TimesheetWorkItem = {
  id: string
  title: string
  issueKey?: string
  hours: number
}

type TimesheetLog = {
  id: string
  projectId: string
  projectName: string
  projectKey: string
  userId: string
  userName: string
  date: string
  hours: number
  workItems: TimesheetWorkItem[]
}

type TimesheetBoard = {
  logs: TimesheetLog[]
  range: { from: string; to: string }
}

type TimesheetFilterOption = {
  label: string
  value: string
  group?: string
}

type TimesheetOptionsSeed = {
  options: TimesheetFilterOption[]
  hasMore?: boolean
}

function readJsonBody(req: Connect.IncomingMessage): Promise<unknown> {
  return new Promise((resolve, reject) => {
    const chunks: Buffer[] = []
    req.on('data', (chunk: Buffer) => chunks.push(chunk))
    req.on('end', () => {
      try {
        const raw = Buffer.concat(chunks).toString('utf8').trim()
        resolve(raw ? JSON.parse(raw) : {})
      } catch (error) {
        reject(error)
      }
    })
    req.on('error', reject)
  })
}

function sendJson(res: ServerResponse, status: number, data: unknown) {
  const body = JSON.stringify(data)
  res.statusCode = status
  res.setHeader('Content-Type', 'application/json; charset=utf-8')
  res.setHeader('Cache-Control', 'no-store')
  res.setHeader('Content-Length', Buffer.byteLength(body))
  res.end(body)
}

function cloneBoard(board: TimesheetBoard): TimesheetBoard {
  return {
    range: { ...board.range },
    logs: board.logs.map((log) => ({
      ...log,
      workItems: log.workItems.map((item) => ({ ...item })),
    })),
  }
}

function inDateRange(date: string, from: string, to: string) {
  return date >= from && date <= to
}

/**
 * Vite middleware mock `/api/v1/timesheet/*`.
 * Seed: `@platform/core` → `features/timesheet/mocks/*.json`
 */
export function timesheetApiMockPlugin(options: {
  boardJsonPath: string
  optionsJsonPath: string
  enabled: boolean
}): Plugin {
  const { boardJsonPath, optionsJsonPath, enabled } = options

  return {
    name: 'sample-timesheet-api-mock',
    configureServer(server) {
      if (!enabled) return

      let store: TimesheetBoard = cloneBoard(
        JSON.parse(fs.readFileSync(boardJsonPath, 'utf8')) as TimesheetBoard,
      )
      let optionsSeed: TimesheetOptionsSeed = JSON.parse(
        fs.readFileSync(optionsJsonPath, 'utf8'),
      ) as TimesheetOptionsSeed
      let seq = 200

      const reloadSeed = () => {
        store = cloneBoard(
          JSON.parse(fs.readFileSync(boardJsonPath, 'utf8')) as TimesheetBoard,
        )
        optionsSeed = JSON.parse(
          fs.readFileSync(optionsJsonPath, 'utf8'),
        ) as TimesheetOptionsSeed
        seq = 200
      }

      try {
        fs.watchFile(boardJsonPath, { interval: 800 }, reloadSeed)
        fs.watchFile(optionsJsonPath, { interval: 800 }, reloadSeed)
      } catch {
        /* ignore */
      }

      server.middlewares.use(async (req, res, next) => {
        const rawUrl = req.url ?? ''
        if (!rawUrl.startsWith('/api/v1/timesheet')) {
          next()
          return
        }

        try {
          const url = new URL(rawUrl, 'http://127.0.0.1')
          const pathname = url.pathname.replace(/\/$/, '') || '/'
          const method = (req.method ?? 'GET').toUpperCase()

          if (method === 'GET' && pathname === '/api/v1/timesheet/board') {
            const from = url.searchParams.get('from') ?? store.range.from
            const to = url.searchParams.get('to') ?? store.range.to
            const search = (url.searchParams.get('search') ?? '')
              .trim()
              .toLowerCase()
            const projectIds = (url.searchParams.get('projectIds') ?? '')
              .split(',')
              .map((s) => s.trim())
              .filter(Boolean)
            const userIds = (url.searchParams.get('userIds') ?? '')
              .split(',')
              .map((s) => s.trim())
              .filter(Boolean)

            let logs = store.logs.filter((log) => inDateRange(log.date, from, to))

            if (projectIds.length) {
              logs = logs.filter((log) => projectIds.includes(log.projectId))
            }
            if (userIds.length) {
              logs = logs.filter((log) => userIds.includes(log.userId))
            }
            if (search) {
              logs = logs.filter(
                (log) =>
                  log.projectName.toLowerCase().includes(search) ||
                  log.projectKey.toLowerCase().includes(search) ||
                  log.userName.toLowerCase().includes(search) ||
                  log.workItems.some((item) =>
                    item.title.toLowerCase().includes(search),
                  ),
              )
            }

            sendJson(res, 200, {
              logs: logs.map((log) => ({
                ...log,
                workItems: log.workItems.map((item) => ({ ...item })),
              })),
              range: { from, to },
            })
            return
          }

          if (method === 'GET' && pathname === '/api/v1/timesheet/options') {
            const search = (url.searchParams.get('search') ?? '')
              .trim()
              .toLowerCase()
            const page = Math.max(1, Number(url.searchParams.get('page') ?? 1))
            const pageSize = Math.max(
              1,
              Number(url.searchParams.get('pageSize') ?? 20),
            )

            let options = [...optionsSeed.options]
            if (search) {
              options = options.filter(
                (opt) =>
                  opt.label.toLowerCase().includes(search) ||
                  opt.value.toLowerCase().includes(search),
              )
            }

            const start = (page - 1) * pageSize
            const slice = options.slice(start, start + pageSize)
            sendJson(res, 200, {
              options: slice,
              hasMore: start + pageSize < options.length,
            })
            return
          }

          if (method === 'PUT' && pathname === '/api/v1/timesheet/hours') {
            const body = (await readJsonBody(req)) as {
              projectId?: string
              userId?: string
              date?: string
              hours?: number
            }
            const projectId = String(body.projectId ?? '')
            const userId = String(body.userId ?? '')
            const date = String(body.date ?? '')
            const hours = Number(body.hours ?? 0)

            if (!projectId || !userId || !date) {
              sendJson(res, 400, { Message: 'Thiếu projectId, userId hoặc date' })
              return
            }

            const idx = store.logs.findIndex(
              (log) =>
                log.projectId === projectId &&
                log.userId === userId &&
                log.date === date,
            )

            if (idx >= 0) {
              const current = store.logs[idx]!
              const next: TimesheetLog = {
                ...current,
                hours,
                workItems:
                  hours > 0
                    ? current.workItems.length
                      ? current.workItems.map((item, i) =>
                          i === 0 ? { ...item, hours } : item,
                        )
                      : [
                          {
                            id: `wi-${++seq}`,
                            title: 'Logged hours',
                            hours,
                          },
                        ]
                    : [],
              }
              store = {
                ...store,
                logs: store.logs.map((log, i) => (i === idx ? next : log)),
              }
              sendJson(res, 200, next)
              return
            }

            const projectOpt = optionsSeed.options.find(
              (opt) => opt.value === `project:${projectId}`,
            )
            const userOpt = optionsSeed.options.find(
              (opt) => opt.value === `user:${userId}`,
            )
            const created: TimesheetLog = {
              id: `ts-${++seq}`,
              projectId,
              projectName: projectOpt?.label ?? projectId,
              projectKey: projectId.slice(-3).toUpperCase(),
              userId,
              userName: userOpt?.label ?? userId,
              date,
              hours,
              workItems:
                hours > 0
                  ? [{ id: `wi-${++seq}`, title: 'Logged hours', hours }]
                  : [],
            }
            store = { ...store, logs: [...store.logs, created] }
            sendJson(res, 200, created)
            return
          }

          if (method === 'POST' && pathname === '/api/v1/timesheet/save') {
            sendJson(res, 200, { saved: store.logs.length })
            return
          }

          sendJson(res, 404, {
            Message: `Timesheet mock: không có route ${method} ${pathname}`,
          })
        } catch (error) {
          sendJson(res, 500, {
            Message:
              error instanceof Error
                ? error.message
                : 'Timesheet mock middleware lỗi',
          })
        }
      })
    },
  }
}
