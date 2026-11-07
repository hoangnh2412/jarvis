import PizZip from 'pizzip'
import { replacePlaceholdersInXml } from './placeholderXml'

const DOCX_XML_PARTS = [
  'word/document.xml',
  'word/header1.xml',
  'word/header2.xml',
  'word/header3.xml',
  'word/footer1.xml',
  'word/footer2.xml',
  'word/footer3.xml',
]

const DOCX_MIME = 'application/vnd.openxmlformats-officedocument.wordprocessingml.document'

export function generateDocument(buffer: ArrayBuffer, data: Record<string, unknown>): Blob {
  const zip = new PizZip(buffer)
  let changed = false

  for (const path of DOCX_XML_PARTS) {
    const file = zip.file(path)
    if (!file) continue
    const current = file.asText()
    const next = replacePlaceholdersInXml(current, data, { keepMissing: true })
    if (next !== current) {
      zip.file(path, next)
      changed = true
    }
  }

  const nextBuffer = changed ? (zip.generate({ type: 'arraybuffer' }) as ArrayBuffer) : buffer
  return new Blob([nextBuffer], { type: DOCX_MIME })
}
