import PizZip from 'pizzip'
import type { DocumentField } from '../types'
import {
  createFieldFromPlaceholder,
  enrichDocumentFields,
  extractPlaceholderNamesFromXml,
  insertPlaceholderInXml,
  mergeExtractedFields,
  removePlaceholderFromXml,
  replacePlaceholdersInXml,
} from '../utils'

const DOCX_XML_PARTS = [
  'word/document.xml',
  'word/header1.xml',
  'word/header2.xml',
  'word/header3.xml',
  'word/footer1.xml',
  'word/footer2.xml',
  'word/footer3.xml',
]

const DOCX_MIME =
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document'

export async function readDocxInput(input: File | Blob): Promise<ArrayBuffer> {
  return input.arrayBuffer()
}

export function parseTemplate(buffer: ArrayBuffer): PizZip {
  return new PizZip(buffer)
}

function readXmlParts(zip: PizZip): Array<{ path: string; xml: string }> {
  return DOCX_XML_PARTS.flatMap((path) => {
    const file = zip.file(path)
    if (!file) return []
    return [{ path, xml: file.asText() }]
  })
}

export function extractFields(
  buffer: ArrayBuffer,
  existing: DocumentField[] = [],
  templateId?: string,
): DocumentField[] {
  const zip = parseTemplate(buffer)
  const names = new Set<string>()

  for (const part of readXmlParts(zip)) {
    extractPlaceholderNamesFromXml(part.xml).forEach((name) => names.add(name))
  }

  const extracted = [...names].map((name) => createFieldFromPlaceholder(name))
  return enrichDocumentFields(mergeExtractedFields(extracted, existing), templateId)
}

function updateXmlParts(
  buffer: ArrayBuffer,
  updater: (xml: string) => string,
): ArrayBuffer {
  const zip = parseTemplate(buffer)
  let changed = false

  for (const path of DOCX_XML_PARTS) {
    const file = zip.file(path)
    if (!file) continue
    const current = file.asText()
    const next = updater(current)
    if (next !== current) {
      zip.file(path, next)
      changed = true
    }
  }

  if (!changed) return buffer

  return zip.generate({ type: 'arraybuffer' }) as ArrayBuffer
}

export function replaceFields(
  buffer: ArrayBuffer,
  data: Record<string, string>,
): Blob {
  const nextBuffer = updateXmlParts(buffer, (xml) =>
    replacePlaceholdersInXml(xml, data, { keepMissing: true }),
  )
  return new Blob([nextBuffer], { type: DOCX_MIME })
}

export function generateDocument(
  buffer: ArrayBuffer,
  data: Record<string, string>,
): Blob {
  return replaceFields(buffer, data)
}

export function removeFieldFromDocument(
  buffer: ArrayBuffer,
  fieldName: string,
): ArrayBuffer {
  return updateXmlParts(buffer, (xml) => removePlaceholderFromXml(xml, fieldName))
}

export function addFieldToDocument(
  buffer: ArrayBuffer,
  placeholder: string,
): ArrayBuffer {
  return updateXmlParts(buffer, (xml) => {
    if (xml.includes(placeholder)) return xml
    return insertPlaceholderInXml(xml, placeholder)
  })
}

export function renameFieldInDocument(
  buffer: ArrayBuffer,
  oldName: string,
  newName: string,
): ArrayBuffer {
  const oldToken = `{{${oldName}}}`
  const newToken = `{{${newName}}}`
  return updateXmlParts(buffer, (xml) => xml.split(oldToken).join(newToken))
}

export async function parseTemplateFile(
  file: File,
  existingFields: DocumentField[] = [],
  templateId?: string,
): Promise<{ buffer: ArrayBuffer; fields: DocumentField[] }> {
  const buffer = await readDocxInput(file)
  const fields = extractFields(buffer, existingFields, templateId)
  return { buffer, fields }
}

export function bufferToBlob(buffer: ArrayBuffer): Blob {
  return new Blob([buffer], { type: DOCX_MIME })
}

export function downloadBlob(blob: Blob, fileName: string): void {
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = fileName.endsWith('.docx') ? fileName : `${fileName}.docx`
  anchor.click()
  URL.revokeObjectURL(url)
}
