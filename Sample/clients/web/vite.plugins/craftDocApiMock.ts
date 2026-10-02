import fs from 'node:fs'
import type { ServerResponse } from 'node:http'
import type { Connect, Plugin } from 'vite'

type CraftDocUploadBody = {
  fileName?: string
  name?: string
  fileBase64?: string
}

type CraftDocTemplateDetailSeed = {
  template: {
    id: string
    name: string
    fileName: string
    updatedAt?: string
    pageSize?: unknown
    marginsTwips?: unknown
    contentWidthTwips?: number
    contentWidthPx96Dpi?: number
  }
  fields: unknown[]
  sampleData: Record<string, string>
}

type CraftDocTemplatesListSeed = {
  templates: Array<{ id: string; name: string; fileName: string }>
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

function sendBinary(
  res: ServerResponse,
  status: number,
  buffer: Buffer,
  contentType: string,
  fileName?: string,
) {
  res.statusCode = status
  res.setHeader('Content-Type', contentType)
  res.setHeader('Cache-Control', 'no-store')
  if (fileName) {
    res.setHeader('Content-Disposition', `inline; filename="${fileName}"`)
  }
  res.setHeader('Content-Length', buffer.length)
  res.end(buffer)
}

function delay(ms: number) {
  return new Promise((resolve) => setTimeout(resolve, ms))
}

function readJsonFile<T>(filePath: string): T {
  return JSON.parse(fs.readFileSync(filePath, 'utf8')) as T
}

/**
 * Vite middleware mock `/api/v1/craft-doc/*`.
 * Seed: `@platform/core` → `features/craftDoc/mocks/*.json` + `mocks/assets/LLA-IDAS.docx`
 */
export function craftDocApiMockPlugin(options: {
  enabled: boolean
  templatesListJsonPath: string
  llaIdasTemplateJsonPath: string
  llaIdasDocxPath: string
}): Plugin {
  const { enabled, templatesListJsonPath, llaIdasTemplateJsonPath, llaIdasDocxPath } =
    options

  return {
    name: 'sample-craft-doc-api-mock',
    configureServer(server) {
      if (!enabled) return

      let templatesList = readJsonFile<CraftDocTemplatesListSeed>(templatesListJsonPath)
      let llaIdasDetail = readJsonFile<CraftDocTemplateDetailSeed>(llaIdasTemplateJsonPath)

      const reloadSeed = () => {
        templatesList = readJsonFile<CraftDocTemplatesListSeed>(templatesListJsonPath)
        llaIdasDetail = readJsonFile<CraftDocTemplateDetailSeed>(llaIdasTemplateJsonPath)
      }

      try {
        fs.watchFile(templatesListJsonPath, { interval: 800 }, reloadSeed)
        fs.watchFile(llaIdasTemplateJsonPath, { interval: 800 }, reloadSeed)
      } catch {
        /* ignore */
      }

      server.middlewares.use(async (req, res, next) => {
        const rawUrl = req.url ?? ''
        if (!rawUrl.startsWith('/api/v1/craft-doc')) {
          next()
          return
        }

        try {
          const url = new URL(rawUrl, 'http://127.0.0.1')
          const pathname = url.pathname.replace(/\/$/, '') || '/'
          const method = (req.method ?? 'GET').toUpperCase()

          if (method === 'GET' && pathname === '/api/v1/craft-doc/templates') {
            await delay(200)
            sendJson(res, 200, templatesList)
            return
          }

          const detailMatch = pathname.match(/^\/api\/v1\/craft-doc\/templates\/([^/]+)$/)
          if (method === 'GET' && detailMatch) {
            const templateId = decodeURIComponent(detailMatch[1] ?? '')
            if (templateId !== 'lla-idas') {
              sendJson(res, 404, {
                Message: `CraftDoc mock: chưa có metadata cho template "${templateId}"`,
              })
              return
            }
            await delay(300)
            sendJson(res, 200, llaIdasDetail)
            return
          }

          const fileMatch = pathname.match(
            /^\/api\/v1\/craft-doc\/templates\/([^/]+)\/file$/,
          )
          if (method === 'GET' && fileMatch) {
            const templateId = decodeURIComponent(fileMatch[1] ?? '')
            if (templateId !== 'lla-idas') {
              sendJson(res, 404, {
                Message: `CraftDoc mock: chưa có file DOCX cho template "${templateId}"`,
              })
              return
            }
            if (!fs.existsSync(llaIdasDocxPath)) {
              sendJson(res, 404, { Message: 'Không tìm thấy LLA-IDAS.docx trong mocks/assets' })
              return
            }
            await delay(400)
            const buffer = fs.readFileSync(llaIdasDocxPath)
            sendBinary(
              res,
              200,
              buffer,
              'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
              'LLA-IDAS.docx',
            )
            return
          }

          const sampleDataMatch = pathname.match(
            /^\/api\/v1\/craft-doc\/templates\/([^/]+)\/sample-data$/,
          )
          if (method === 'GET' && sampleDataMatch) {
            const templateId = decodeURIComponent(sampleDataMatch[1] ?? '')
            if (templateId !== 'lla-idas') {
              sendJson(res, 404, {
                Message: `CraftDoc mock: chưa có sample-data cho template "${templateId}"`,
              })
              return
            }
            await delay(150)
            sendJson(res, 200, { sampleData: llaIdasDetail.sampleData })
            return
          }

          if (method === 'POST' && pathname === '/api/v1/craft-doc/templates/upload') {
            const body = (await readJsonBody(req)) as CraftDocUploadBody
            const fileBase64 = String(body.fileBase64 ?? '')
            const fileName = String(body.fileName ?? 'upload.docx')
            const name = String(body.name ?? fileName.replace(/\.docx$/i, ''))

            if (!fileBase64) {
              sendJson(res, 400, { Message: 'Thiếu fileBase64' })
              return
            }

            await delay(700)

            const isLlaIdas =
              fileName.trim().toLowerCase().replace(/\.docx$/i, '') === 'lla-idas'

            if (isLlaIdas) {
              sendJson(res, 200, {
                id: llaIdasDetail.template.id,
                name: llaIdasDetail.template.name,
                fileName,
                updatedAt:
                  llaIdasDetail.template.updatedAt ?? new Date().toISOString(),
                fileBase64,
                fields: llaIdasDetail.fields,
                sampleData: llaIdasDetail.sampleData,
              })
              return
            }

            sendJson(res, 200, {
              id: `upload-${Date.now()}`,
              name,
              fileName,
              updatedAt: new Date().toISOString(),
              fileBase64,
            })
            return
          }

          sendJson(res, 404, {
            Message: `CraftDoc mock: không có route ${method} ${pathname}`,
          })
        } catch (error) {
          sendJson(res, 500, {
            Message:
              error instanceof Error ? error.message : 'CraftDoc mock middleware lỗi',
          })
        }
      })
    },
  }
}
