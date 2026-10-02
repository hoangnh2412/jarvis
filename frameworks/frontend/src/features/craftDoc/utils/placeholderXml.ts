const WT_REGEX = /<w:t(\s[^>]*)?>([^<]*)<\/w:t>/g
const PLACEHOLDER_REGEX = /\{\{([^}]+)\}\}/g

export function escapeXmlText(text: string): string {
  return text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
}

type TextSegment = {
  attr: string
  text: string
}

function collectTextSegments(xml: string): TextSegment[] {
  const segments: TextSegment[] = []
  let match: RegExpExecArray | null
  const regex = new RegExp(WT_REGEX.source, 'g')
  while ((match = regex.exec(xml)) !== null) {
    segments.push({ attr: match[1] ?? '', text: match[2] })
  }
  return segments
}

function rebuildXmlWithSegments(xml: string, segments: TextSegment[]): string {
  let index = 0
  return xml.replace(new RegExp(WT_REGEX.source, 'g'), () => {
    const segment = segments[index++]
    if (!segment) return ''
    return `<w:t${segment.attr}>${escapeXmlText(segment.text)}</w:t>`
  })
}

function getCombinedText(segments: TextSegment[]): string {
  return segments.map((segment) => segment.text).join('')
}

function findSegmentRange(
  segments: TextSegment[],
  start: number,
  end: number,
): { firstIdx: number; lastIdx: number } | null {
  let cursor = 0
  let firstIdx = -1
  let lastIdx = -1

  for (let i = 0; i < segments.length; i += 1) {
    const partStart = cursor
    const partEnd = cursor + segments[i].text.length
    if (partEnd > start && partStart < end) {
      if (firstIdx === -1) firstIdx = i
      lastIdx = i
    }
    cursor = partEnd
  }

  if (firstIdx === -1 || lastIdx === -1) return null
  return { firstIdx, lastIdx }
}

function charOffsetBeforeSegment(segments: TextSegment[], segmentIndex: number): number {
  let offset = 0
  for (let i = 0; i < segmentIndex; i += 1) {
    offset += segments[i].text.length
  }
  return offset
}

function applySpanReplacement(
  segments: TextSegment[],
  start: number,
  end: number,
  value: string,
): void {
  const range = findSegmentRange(segments, start, end)
  if (!range) return

  const firstOffset = charOffsetBeforeSegment(segments, range.firstIdx)
  const lastOffset = charOffsetBeforeSegment(segments, range.lastIdx)
  const localStart = start - firstOffset
  const localEnd = end - lastOffset

  const firstText = segments[range.firstIdx].text
  const lastText = segments[range.lastIdx].text
  segments[range.firstIdx].text =
    firstText.slice(0, localStart) + value + lastText.slice(localEnd)

  for (let i = range.firstIdx + 1; i <= range.lastIdx; i += 1) {
    segments[i].text = ''
  }
}

export function extractPlaceholderNamesFromXml(xml: string): string[] {
  const combined = getCombinedText(collectTextSegments(xml))
  const names = new Set<string>()
  let match: RegExpExecArray | null
  const regex = new RegExp(PLACEHOLDER_REGEX.source, 'g')
  while ((match = regex.exec(combined)) !== null) {
    names.add(match[1].trim())
  }
  return [...names]
}

export function replacePlaceholdersInXml(
  xml: string,
  data: Record<string, string>,
  options?: { keepMissing?: boolean },
): string {
  const segments = collectTextSegments(xml)
  if (!segments.length) return xml

  const combined = getCombinedText(segments)
  const replacements: Array<{ start: number; end: number; value: string }> = []
  let match: RegExpExecArray | null
  const regex = new RegExp(PLACEHOLDER_REGEX.source, 'g')

  while ((match = regex.exec(combined)) !== null) {
    const key = match[1].trim()
    if (!(key in data)) {
      if (options?.keepMissing) continue
      continue
    }
    const value = data[key] ?? ''
    if (!value.trim() && options?.keepMissing) continue
    replacements.push({
      start: match.index,
      end: match.index + match[0].length,
      value,
    })
  }

  replacements.sort((a, b) => b.start - a.start)
  for (const replacement of replacements) {
    applySpanReplacement(segments, replacement.start, replacement.end, replacement.value)
  }

  return rebuildXmlWithSegments(xml, segments)
}

export function removePlaceholderFromXml(xml: string, fieldName: string): string {
  const token = `{{${fieldName}}}`
  const segments = collectTextSegments(xml)
  if (!segments.length) return xml

  const combined = getCombinedText(segments)
  let searchFrom = 0

  while (true) {
    const index = combined.indexOf(token, searchFrom)
    if (index === -1) break
    applySpanReplacement(segments, index, index + token.length, '')
    searchFrom = index
  }

  return rebuildXmlWithSegments(xml, segments)
}

export function insertPlaceholderInXml(xml: string, placeholder: string): string {
  const token = placeholder.startsWith('{{') ? placeholder : `{{${placeholder}}}`
  const bodyClose = '</w:body>'
  const insertXml = `<w:p><w:r><w:t xml:space="preserve">${escapeXmlText(token)}</w:t></w:r></w:p>`

  if (xml.includes(bodyClose)) {
    return xml.replace(bodyClose, `${insertXml}${bodyClose}`)
  }

  return `${xml}${insertXml}`
}

const WR_BLOCK_REGEX = /<w:r\b[^>]*>[\s\S]*?<\/w:r>/g
const WP_BLOCK_REGEX = /<w:p\b[^>]*>[\s\S]*?<\/w:p>/g
const RPR_REGEX = /<w:rPr[\s\S]*?<\/w:rPr>/
const RUN_INNER_TOKEN_REGEX = /<w:t(?:\s[^>]*)?>([^<]*)<\/w:t>|<w:tab\s*\/>/g

type ParsedRun = {
  xml: string
  inner: string
  text: string
  start: number
  end: number
}

function extractRunInner(runXml: string): string {
  return runXml.replace(/^<w:r\b[^>]*>/, '').replace(/<\/w:r>$/, '')
}

function extractTextFromRunInner(runInner: string): string {
  let text = ''
  const regex = new RegExp(RUN_INNER_TOKEN_REGEX.source, 'g')
  let match: RegExpExecArray | null
  while ((match = regex.exec(runInner)) !== null) {
    if (match[0].startsWith('<w:tab')) {
      text += '\t'
    } else {
      text += match[1]
    }
  }
  return text
}

function extractRunRPr(runInner: string): string {
  return runInner.match(RPR_REGEX)?.[0] ?? ''
}

function pickBestRunInner(runs: ParsedRun[]): string {
  const withBold = runs.find((run) => run.inner.includes('<w:b'))
  if (withBold) return withBold.inner
  return runs.reduce((best, run) =>
    run.text.length > best.text.length ? run : best,
  ).inner
}

function buildRunXmlFromInner(runInner: string, text: string): string {
  const rPr = extractRunRPr(runInner)
  const preserve = /^\s|\s$/.test(text) ? ' xml:space="preserve"' : ''
  return `<w:r>${rPr}<w:t${preserve}>${escapeXmlText(text)}</w:t></w:r>`
}

function parseParagraphRuns(paragraphInner: string): {
  prefix: string
  runs: ParsedRun[]
  suffix: string
} {
  const runs: ParsedRun[] = []
  let prefix = ''
  let suffix = ''
  let lastEnd = 0
  let offset = 0
  let firstRun = true
  const regex = new RegExp(WR_BLOCK_REGEX.source, 'g')
  let match: RegExpExecArray | null

  while ((match = regex.exec(paragraphInner)) !== null) {
    if (firstRun) {
      prefix = paragraphInner.slice(0, match.index)
      firstRun = false
    }
    const xml = match[0]
    const inner = extractRunInner(xml)
    const text = extractTextFromRunInner(inner)
    runs.push({
      xml,
      inner,
      text,
      start: offset,
      end: offset + text.length,
    })
    offset += text.length
    lastEnd = match.index + xml.length
  }

  suffix = paragraphInner.slice(lastEnd)
  return { prefix, runs, suffix }
}

function mergeSplitPlaceholderRunsInParagraph(paragraphInner: string): string {
  const { prefix, runs, suffix } = parseParagraphRuns(paragraphInner)
  if (runs.length <= 1) return paragraphInner

  const combined = runs.map((run) => run.text).join('')
  const mergeRanges: Array<{ startIdx: number; endIdx: number }> = []
  let match: RegExpExecArray | null
  const regex = new RegExp(PLACEHOLDER_REGEX.source, 'g')

  while ((match = regex.exec(combined)) !== null) {
    const pStart = match.index
    const pEnd = match.index + match[0].length
    const involved = runs.filter((run) => run.end > pStart && run.start < pEnd)
    if (involved.length <= 1) continue
    mergeRanges.push({
      startIdx: runs.indexOf(involved[0]),
      endIdx: runs.indexOf(involved[involved.length - 1]),
    })
  }

  if (!mergeRanges.length) return paragraphInner

  mergeRanges.sort((a, b) => b.startIdx - a.startIdx)
  for (const { startIdx, endIdx } of mergeRanges) {
    const slice = runs.slice(startIdx, endIdx + 1)
    const mergedText = combined.slice(slice[0].start, slice[slice.length - 1].end)
    const mergedInner = pickBestRunInner(slice)
    runs.splice(startIdx, endIdx - startIdx + 1, {
      xml: buildRunXmlFromInner(mergedInner, mergedText),
      inner: mergedInner,
      text: mergedText,
      start: slice[0].start,
      end: slice[0].start + mergedText.length,
    })
  }

  let cursor = 0
  for (const run of runs) {
    run.start = cursor
    cursor += run.text.length
    run.end = cursor
  }

  return `${prefix}${runs.map((run) => run.xml).join('')}${suffix}`
}

/** Gộp placeholder bị Word tách thành nhiều w:r — giảm khoảng cách thừa khi preview. */
export function normalizeSplitPlaceholdersInXml(xml: string): string {
  return xml.replace(WP_BLOCK_REGEX, (paragraph) => {
    const openTag = paragraph.match(/^<w:p\b[^>]*>/)?.[0] ?? '<w:p>'
    const inner = paragraph.slice(openTag.length, paragraph.length - '</w:p>'.length)
    const normalizedInner = mergeSplitPlaceholderRunsInParagraph(inner)
    return `${openTag}${normalizedInner}</w:p>`
  })
}

/** Xóa w:r rỗng sau thay thế placeholder — tránh khoảng trắng ảo trong preview. */
export function removeEmptyRunsInXml(xml: string): string {
  return xml.replace(WR_BLOCK_REGEX, (runXml) => {
    const inner = extractRunInner(runXml)
    return extractTextFromRunInner(inner) ? runXml : ''
  })
}

/** Chuẩn hóa XML document trước preview/export. */
export function normalizeDocxXml(xml: string): string {
  return removeEmptyRunsInXml(normalizeSplitPlaceholdersInXml(xml))
}
