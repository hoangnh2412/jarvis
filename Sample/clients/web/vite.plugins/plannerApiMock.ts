import fs from 'node:fs'
import type { ServerResponse } from 'node:http'
import type { Connect, Plugin } from 'vite'

type PlannerColumn = {
  id: string
  name: string
  order: number
  color?: string
}

type PlannerItem = {
  id: string
  title: string
  description?: string | null
  statusId: string
  start: string
  end?: string | null
  allDay?: boolean
  priority?: 'low' | 'medium' | 'high'
  assignee?: string | null
  updatedAt?: string
}

type PlannerBoard = {
  columns: PlannerColumn[]
  items: PlannerItem[]
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

function sendJson(
  res: ServerResponse,
  status: number,
  data: unknown,
) {
  const body = JSON.stringify(data)
  res.statusCode = status
  res.setHeader('Content-Type', 'application/json; charset=utf-8')
  res.setHeader('Cache-Control', 'no-store')
  res.setHeader('Content-Length', Buffer.byteLength(body))
  res.end(body)
}

function cloneBoard(board: PlannerBoard): PlannerBoard {
  return {
    columns: board.columns.map((c) => ({ ...c })),
    items: board.items.map((item) => ({ ...item })),
  }
}

/**
 * Vite middleware mock `/api/v1/planner/*`.
 * Tránh MSW Service Worker — DevTools Network không bị treo / ERR_CACHE_MISS.
 * Seed: `@platform/core` → `features/planner/mocks/get-board.json`
 */
export function plannerApiMockPlugin(options: {
  boardJsonPath: string
  enabled: boolean
}): Plugin {
  const { boardJsonPath, enabled } = options

  return {
    name: 'sample-planner-api-mock',
    configureServer(server) {
      if (!enabled) return

      let store: PlannerBoard = cloneBoard(
        JSON.parse(fs.readFileSync(boardJsonPath, 'utf8')) as PlannerBoard,
      )
      let seq = 100

      const reloadSeed = () => {
        store = cloneBoard(
          JSON.parse(fs.readFileSync(boardJsonPath, 'utf8')) as PlannerBoard,
        )
        seq = 100
      }

      try {
        fs.watchFile(boardJsonPath, { interval: 800 }, () => {
          reloadSeed()
        })
      } catch {
        /* ignore */
      }

      server.middlewares.use(async (req, res, next) => {
        const rawUrl = req.url ?? ''
        if (!rawUrl.startsWith('/api/v1/planner')) {
          next()
          return
        }

        try {
          const url = new URL(rawUrl, 'http://127.0.0.1')
          const pathname = url.pathname.replace(/\/$/, '') || '/'
          const method = (req.method ?? 'GET').toUpperCase()

          if (method === 'GET' && pathname === '/api/v1/planner/board') {
            const search = (url.searchParams.get('search') ?? '')
              .trim()
              .toLowerCase()
            const priority = url.searchParams.get('priority') ?? 'all'
            let items = store.items.map((item) => ({ ...item }))
            if (search) {
              items = items.filter(
                (item) =>
                  item.title.toLowerCase().includes(search) ||
                  (item.description ?? '').toLowerCase().includes(search) ||
                  (item.assignee ?? '').toLowerCase().includes(search),
              )
            }
            if (priority && priority !== 'all') {
              items = items.filter((item) => item.priority === priority)
            }
            sendJson(res, 200, {
              columns: store.columns.map((c) => ({ ...c })),
              items,
            })
            return
          }

          if (method === 'POST' && pathname === '/api/v1/planner/items') {
            const body = (await readJsonBody(req)) as Partial<PlannerItem>
            const now = new Date().toISOString()
            const item: PlannerItem = {
              id: `pln-${++seq}`,
              title: String(body.title ?? '').trim(),
              description: body.description ?? null,
              statusId: String(body.statusId ?? store.columns[0]?.id ?? 'todo'),
              start: String(body.start ?? now),
              end: body.end ?? null,
              allDay: Boolean(body.allDay),
              priority: body.priority ?? 'medium',
              assignee: body.assignee?.trim() ? body.assignee.trim() : null,
              updatedAt: now,
            }
            store = { ...store, items: [item, ...store.items] }
            sendJson(res, 201, item)
            return
          }

          const moveMatch = pathname.match(
            /^\/api\/v1\/planner\/items\/([^/]+)\/move$/,
          )
          if (method === 'PUT' && moveMatch) {
            const id = decodeURIComponent(moveMatch[1])
            const body = (await readJsonBody(req)) as { statusId?: string }
            const current = store.items.find((item) => item.id === id)
            if (!current) {
              sendJson(res, 404, { Message: `Không tìm thấy item: ${id}` })
              return
            }
            const next = {
              ...current,
              statusId: body.statusId ?? current.statusId,
              updatedAt: new Date().toISOString(),
            }
            store = {
              ...store,
              items: store.items.map((item) =>
                item.id === id ? next : item,
              ),
            }
            sendJson(res, 200, next)
            return
          }

          const rescheduleMatch = pathname.match(
            /^\/api\/v1\/planner\/items\/([^/]+)\/reschedule$/,
          )
          if (method === 'PUT' && rescheduleMatch) {
            const id = decodeURIComponent(rescheduleMatch[1])
            const body = (await readJsonBody(req)) as Partial<PlannerItem>
            const current = store.items.find((item) => item.id === id)
            if (!current) {
              sendJson(res, 404, { Message: `Không tìm thấy item: ${id}` })
              return
            }
            const next = {
              ...current,
              start: body.start ?? current.start,
              end: body.end !== undefined ? body.end : current.end,
              allDay: body.allDay ?? current.allDay,
              updatedAt: new Date().toISOString(),
            }
            store = {
              ...store,
              items: store.items.map((item) =>
                item.id === id ? next : item,
              ),
            }
            sendJson(res, 200, next)
            return
          }

          const itemMatch = pathname.match(
            /^\/api\/v1\/planner\/items\/([^/]+)$/,
          )
          if (method === 'PUT' && itemMatch) {
            const id = decodeURIComponent(itemMatch[1])
            const body = (await readJsonBody(req)) as Partial<PlannerItem>
            const current = store.items.find((item) => item.id === id)
            if (!current) {
              sendJson(res, 404, { Message: `Không tìm thấy item: ${id}` })
              return
            }
            const next: PlannerItem = {
              ...current,
              ...body,
              id,
              title:
                body.title !== undefined
                  ? String(body.title).trim()
                  : current.title,
              updatedAt: new Date().toISOString(),
            }
            store = {
              ...store,
              items: store.items.map((item) =>
                item.id === id ? next : item,
              ),
            }
            sendJson(res, 200, next)
            return
          }

          if (method === 'DELETE' && itemMatch) {
            const id = decodeURIComponent(itemMatch[1])
            if (!store.items.some((item) => item.id === id)) {
              sendJson(res, 404, { Message: `Không tìm thấy item: ${id}` })
              return
            }
            store = {
              ...store,
              items: store.items.filter((item) => item.id !== id),
            }
            sendJson(res, 200, { id })
            return
          }

          sendJson(res, 404, {
            Message: `Planner mock: không có route ${method} ${pathname}`,
          })
        } catch (error) {
          sendJson(res, 500, {
            Message:
              error instanceof Error
                ? error.message
                : 'Planner mock middleware lỗi',
          })
        }
      })
    },
  }
}
