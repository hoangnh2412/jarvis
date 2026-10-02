import PizZip from 'pizzip'

const CONTENT_TYPES = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
  <Override PartName="/word/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml"/>
  <Override PartName="/word/fontTable.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.fontTable+xml"/>
  <Override PartName="/word/settings.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.settings+xml"/>
</Types>`

const ROOT_RELS = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
</Relationships>`

const DOCUMENT_RELS = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/settings" Target="settings.xml"/>
</Relationships>`

const STYLES_XML = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:styles xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:docDefaults>
    <w:rPrDefault>
      <w:rPr>
        <w:rFonts w:ascii="Times New Roman" w:eastAsia="Times New Roman" w:hAnsi="Times New Roman" w:cs="Times New Roman"/>
        <w:sz w:val="26"/>
        <w:szCs w:val="26"/>
        <w:lang w:val="vi-VN" w:eastAsia="vi-VN" w:bidi="vi-VN"/>
      </w:rPr>
    </w:rPrDefault>
    <w:pPrDefault>
      <w:pPr>
        <w:spacing w:after="120" w:line="360" w:lineRule="auto"/>
      </w:pPr>
    </w:pPrDefault>
  </w:docDefaults>
  <w:style w:type="paragraph" w:default="1" w:styleId="Normal">
    <w:name w:val="Normal"/>
    <w:qFormat/>
    <w:pPr>
      <w:spacing w:after="120" w:line="360" w:lineRule="auto"/>
    </w:pPr>
    <w:rPr>
      <w:rFonts w:ascii="Times New Roman" w:eastAsia="Times New Roman" w:hAnsi="Times New Roman" w:cs="Times New Roman"/>
      <w:sz w:val="26"/>
      <w:szCs w:val="26"/>
    </w:rPr>
  </w:style>
</w:styles>`

const FONT_TABLE_XML = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:fonts xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:font w:name="Times New Roman">
    <w:panose1 w:val="02020603050405020304"/>
    <w:charset w:val="00"/>
    <w:family w:val="roman"/>
    <w:pitch w:val="variable"/>
    <w:sig w:usb0="E0002EFF" w:usb1="C000785B" w:usb2="00000009" w:usb3="00000000" w:csb0="000001FF" w:csb1="00000000"/>
  </w:font>
</w:fonts>`

const SETTINGS_XML = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:settings xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:defaultTabStop w:val="720"/>
  <w:characterSpacingControl w:val="doNotCompress"/>
  <w:compat>
    <w:compatSetting w:name="compatibilityMode" w:uri="http://schemas.microsoft.com/office/word" w:val="15"/>
  </w:compat>
</w:settings>`

/** US Letter — lề 1 inch (2.54cm) mỗi cạnh (giống LLA-IDAS.docx) */
const PAGE = {
  width: 12240,
  height: 16840,
  marginTop: 1440,
  marginRight: 1440,
  marginBottom: 1440,
  marginLeft: 1440,
} as const

const CONTENT_WIDTH = PAGE.width - PAGE.marginLeft - PAGE.marginRight
const COL_WIDTH = Math.floor(CONTENT_WIDTH / 2)

const FONT = 'Times New Roman'
const FONT_BODY = 26
const FONT_TITLE = 28
const LINE_15 = 360
const INDENT_FIRST = 720
const INDENT_LIST = 720

export type ParagraphAlign = 'left' | 'center' | 'right' | 'both'

export type ParagraphDef = {
  text: string
  align?: ParagraphAlign
  bold?: boolean
  italic?: boolean
  fontSize?: number
  spacingBefore?: number
  spacingAfter?: number
  lineSpacing?: number
  indentLeft?: number
  indentFirstLine?: number
  underline?: boolean
}

export type TableRowDef = {
  left: string
  right: string
  align?: ParagraphAlign
  bold?: boolean
  italic?: boolean
  fontSize?: number
  spacingBefore?: number
  spacingAfter?: number
}

export type TableDef = {
  type: 'table'
  rows: TableRowDef[]
}

export type BlockDef = ParagraphDef | TableDef

function isTableDef(block: BlockDef): block is TableDef {
  return 'type' in block && block.type === 'table'
}

function escapeXml(text: string): string {
  return text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
}

function buildPPr(def: ParagraphDef): string {
  const parts: string[] = ['<w:pStyle w:val="Normal"/>']

  if (def.align) parts.push(`<w:jc w:val="${def.align}"/>`)
  if (def.indentLeft || def.indentFirstLine) {
    parts.push(
      `<w:ind${def.indentLeft ? ` w:left="${def.indentLeft}"` : ''}${
        def.indentFirstLine ? ` w:firstLine="${def.indentFirstLine}"` : ''
      }/>`,
    )
  }
  if (def.spacingBefore || def.spacingAfter || def.lineSpacing) {
    const attrs = [
      def.spacingBefore ? `w:before="${def.spacingBefore}"` : '',
      def.spacingAfter ? `w:after="${def.spacingAfter}"` : '',
      def.lineSpacing ? `w:line="${def.lineSpacing}" w:lineRule="auto"` : '',
    ]
      .filter(Boolean)
      .join(' ')
    parts.push(`<w:spacing ${attrs}/>`)
  }
  if (def.underline) {
    parts.push(
      '<w:pBdr><w:bottom w:val="single" w:sz="6" w:space="1" w:color="auto"/></w:pBdr>',
    )
  }

  return parts.length ? `<w:pPr>${parts.join('')}</w:pPr>` : ''
}

function buildRPr(def: Pick<ParagraphDef, 'bold' | 'italic' | 'fontSize'>): string {
  const parts: string[] = [
    `<w:rFonts w:ascii="${FONT}" w:eastAsia="${FONT}" w:hAnsi="${FONT}" w:cs="${FONT}"/>`,
    `<w:lang w:val="vi-VN" w:eastAsia="vi-VN"/>`,
  ]
  if (def.bold) parts.push('<w:b/><w:bCs/>')
  if (def.italic) parts.push('<w:i/><w:iCs/>')
  const size = def.fontSize ?? FONT_BODY
  parts.push(`<w:sz w:val="${size}"/><w:szCs w:val="${size}"/>`)
  return `<w:rPr>${parts.join('')}</w:rPr>`
}

function runsFromText(text: string, def: ParagraphDef): string {
  const placeholderPattern = /\{\{[^}]+\}\}/g
  const parts: string[] = []
  let lastIndex = 0
  let match: RegExpExecArray | null
  const rPr = buildRPr(def)

  while ((match = placeholderPattern.exec(text)) !== null) {
    if (match.index > lastIndex) {
      parts.push(
        `<w:r>${rPr}<w:t xml:space="preserve">${escapeXml(text.slice(lastIndex, match.index))}</w:t></w:r>`,
      )
    }
    parts.push(
      `<w:r>${rPr}<w:t xml:space="preserve">${escapeXml(match[0])}</w:t></w:r>`,
    )
    lastIndex = match.index + match[0].length
  }

  if (lastIndex < text.length) {
    parts.push(
      `<w:r>${rPr}<w:t xml:space="preserve">${escapeXml(text.slice(lastIndex))}</w:t></w:r>`,
    )
  }

  if (parts.length === 0) {
    parts.push(`<w:r>${rPr}<w:t xml:space="preserve">${escapeXml(text)}</w:t></w:r>`)
  }

  return parts.join('')
}

function paragraph(def: ParagraphDef): string {
  const pPr = buildPPr(
    def.text
      ? def
      : { ...def, spacingAfter: def.spacingAfter ?? 120, lineSpacing: def.lineSpacing ?? LINE_15 },
  )
  if (!def.text) return `<w:p>${pPr}</w:p>`
  return `<w:p>${pPr}${runsFromText(def.text, def)}</w:p>`
}

function tableCell(text: string, row: TableRowDef): string {
  const cellDef: ParagraphDef = {
    text,
    align: row.align ?? 'center',
    bold: row.bold,
    italic: row.italic,
    fontSize: row.fontSize,
    spacingBefore: row.spacingBefore,
    spacingAfter: row.spacingAfter ?? 120,
    lineSpacing: LINE_15,
  }

  return `<w:tc>
    <w:tcPr>
      <w:tcW w:w="${COL_WIDTH}" w:type="dxa"/>
      <w:vAlign w:val="top"/>
    </w:tcPr>
    ${paragraph(cellDef)}
  </w:tc>`
}

function table(def: TableDef): string {
  const rows = def.rows
    .map(
      (row) =>
        `<w:tr>${tableCell(row.left, row)}${tableCell(row.right, row)}</w:tr>`,
    )
    .join('')

  return `<w:tbl>
    <w:tblPr>
      <w:tblW w:w="${CONTENT_WIDTH}" w:type="dxa"/>
      <w:tblLayout w:type="fixed"/>
      <w:tblLook w:val="04A0" w:firstRow="0" w:lastRow="0" w:firstColumn="0" w:lastColumn="0" w:noHBand="1" w:noVBand="1"/>
      <w:tblBorders>
        <w:top w:val="nil"/><w:left w:val="nil"/><w:bottom w:val="nil"/><w:right w:val="nil"/>
        <w:insideH w:val="nil"/><w:insideV w:val="nil"/>
      </w:tblBorders>
    </w:tblPr>
    <w:tblGrid>
      <w:gridCol w:w="${COL_WIDTH}"/>
      <w:gridCol w:w="${CONTENT_WIDTH - COL_WIDTH}"/>
    </w:tblGrid>
    ${rows}
  </w:tbl>`
}

function blockToXml(block: BlockDef): string {
  return isTableDef(block) ? table(block) : paragraph(block)
}

function buildDocumentXml(blocks: BlockDef[]): string {
  const body = blocks.map(blockToXml).join('')

  return `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:body>
    ${body}
    <w:sectPr>
      <w:pgSz w:w="${PAGE.width}" w:h="${PAGE.height}"/>
      <w:pgMar w:top="${PAGE.marginTop}" w:right="${PAGE.marginRight}" w:bottom="${PAGE.marginBottom}" w:left="${PAGE.marginLeft}"/>
    </w:sectPr>
  </w:body>
</w:document>`
}

function packDocx(documentXml: string): Blob {
  const zip = new PizZip()
  zip.file('[Content_Types].xml', CONTENT_TYPES)
  zip.file('_rels/.rels', ROOT_RELS)
  zip.file('word/_rels/document.xml.rels', DOCUMENT_RELS)
  zip.file('word/document.xml', documentXml)
  zip.file('word/styles.xml', STYLES_XML)
  zip.file('word/fontTable.xml', FONT_TABLE_XML)
  zip.file('word/settings.xml', SETTINGS_XML)

  return zip.generate({
    type: 'blob',
    mimeType:
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  }) as Blob
}

function linesToParagraphs(lines: string[]): ParagraphDef[] {
  return lines.map((line) => ({
    text: line,
    align: 'both' as const,
    lineSpacing: LINE_15,
    spacingAfter: 120,
  }))
}

export function createDocxBlob(title: string, lines: string[]): Blob {
  const titleLines = title.split('\n').filter((line) => line.trim())
  const blocks: BlockDef[] = [
    ...titleLines.map((line, index) => ({
      text: line,
      align: 'center' as const,
      bold: index === 0 || line.includes('HỢP ĐỒNG'),
      fontSize: line.includes('HỢP ĐỒNG') ? FONT_TITLE : FONT_BODY,
      lineSpacing: LINE_15,
      spacingAfter: 120,
    })),
    { text: '', spacingAfter: 120 },
    ...linesToParagraphs(lines),
  ]

  return createDocxFromBlocks(blocks)
}

export function createDocxFromParagraphs(paragraphs: ParagraphDef[]): Blob {
  return createDocxFromBlocks(paragraphs)
}

export function createDocxFromBlocks(blocks: BlockDef[]): Blob {
  return packDocx(buildDocumentXml(blocks))
}

export type SampleDocxDefinition = {
  id: string
  name: string
  fileName: string
  blocks: BlockDef[]
}

/** Một mẫu duy nhất — Hợp đồng lao động xác định thời hạn (VN) */
export const SAMPLE_DOCX_DEFINITIONS: SampleDocxDefinition[] = [
  {
    id: 'labor-contract',
    name: 'Hợp đồng lao động',
    fileName: 'Hop-dong-lao-dong-mau.docx',
    blocks: [
      {
        text: 'CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM',
        align: 'center',
        bold: true,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: 'Độc lập - Tự do - Hạnh phúc',
        align: 'center',
        bold: true,
        underline: true,
        spacingAfter: 280,
        lineSpacing: LINE_15,
      },
      {
        text: 'HỢP ĐỒNG LAO ĐỘNG',
        align: 'center',
        bold: true,
        fontSize: FONT_TITLE,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: '(Loại hợp đồng: {{contractType}})',
        align: 'center',
        italic: true,
        spacingAfter: 160,
        lineSpacing: LINE_15,
      },
      {
        text: 'Số: {{contractNumber}}',
        align: 'center',
        spacingAfter: 240,
        lineSpacing: LINE_15,
      },
      {
        text: 'Căn cứ Bộ luật Lao động số 45/2019/QH14;',
        align: 'both',
        indentFirstLine: INDENT_FIRST,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: 'Căn cứ nhu cầu sử dụng lao động của doanh nghiệp.',
        align: 'both',
        indentFirstLine: INDENT_FIRST,
        spacingAfter: 240,
        lineSpacing: LINE_15,
      },
      {
        text: 'Hôm nay, ngày {{contractDate}}, tại {{signPlace}}, chúng tôi gồm:',
        align: 'both',
        indentFirstLine: INDENT_FIRST,
        spacingAfter: 180,
        lineSpacing: LINE_15,
      },
      {
        text: 'BÊN SỬ DỤNG LAO ĐỘNG (Bên A): {{legalEntity}}',
        align: 'both',
        bold: true,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: 'Địa chỉ: {{employerAddress}}',
        align: 'both',
        indentLeft: INDENT_LIST,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: 'Đại diện: {{employerRepresentative}} — Chức vụ: {{employerTitle}}',
        align: 'both',
        indentLeft: INDENT_LIST,
        spacingAfter: 180,
        lineSpacing: LINE_15,
      },
      {
        text: 'BÊN NGƯỜI LAO ĐỘNG (Bên B): {{fullName}}',
        align: 'both',
        bold: true,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: 'Sinh ngày: {{birthDate}}',
        align: 'both',
        indentLeft: INDENT_LIST,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: 'CCCD số: {{identityNumber}} — Cấp ngày: {{identityIssueDate}} — Nơi cấp: {{identityIssuePlace}}',
        align: 'both',
        indentLeft: INDENT_LIST,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: 'Địa chỉ thường trú: {{permanentAddress}}',
        align: 'both',
        indentLeft: INDENT_LIST,
        spacingAfter: 240,
        lineSpacing: LINE_15,
      },
      {
        text: 'Hai bên thống nhất ký kết hợp đồng lao động với các điều khoản sau:',
        align: 'both',
        indentFirstLine: INDENT_FIRST,
        spacingAfter: 240,
        lineSpacing: LINE_15,
      },
      {
        text: 'Điều 1. Công việc và địa điểm làm việc',
        align: 'both',
        bold: true,
        spacingBefore: 120,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: 'Chức danh: {{position}}',
        align: 'both',
        indentLeft: INDENT_LIST,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: 'Phòng ban: {{department}}',
        align: 'both',
        indentLeft: INDENT_LIST,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: 'Nơi làm việc: {{workLocation}}',
        align: 'both',
        indentLeft: INDENT_LIST,
        spacingAfter: 180,
        lineSpacing: LINE_15,
      },
      {
        text: 'Điều 2. Thời hạn hợp đồng',
        align: 'both',
        bold: true,
        spacingBefore: 120,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: 'Thời gian thử việc: {{probationDays}} ngày',
        align: 'both',
        indentLeft: INDENT_LIST,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: 'Thời hạn hợp đồng: từ ngày {{startDate}} đến ngày {{endDate}}',
        align: 'both',
        indentLeft: INDENT_LIST,
        spacingAfter: 180,
        lineSpacing: LINE_15,
      },
      {
        text: 'Điều 3. Chế độ làm việc',
        align: 'both',
        bold: true,
        spacingBefore: 120,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: '{{workingHours}}',
        align: 'both',
        indentLeft: INDENT_LIST,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: 'Bên B được hưởng các chế độ nghỉ lễ, nghỉ phép theo quy định pháp luật.',
        align: 'both',
        indentLeft: INDENT_LIST,
        spacingAfter: 180,
        lineSpacing: LINE_15,
      },
      {
        text: 'Điều 4. Tiền lương và phương thức trả lương',
        align: 'both',
        bold: true,
        spacingBefore: 120,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: 'Mức lương cơ bản: {{baseSalary}} VNĐ/tháng',
        align: 'both',
        indentLeft: INDENT_LIST,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: 'Hình thức trả lương: {{salaryPaymentMethod}}',
        align: 'both',
        indentLeft: INDENT_LIST,
        spacingAfter: 180,
        lineSpacing: LINE_15,
      },
      {
        text: 'Điều 5. Quyền và nghĩa vụ của các bên',
        align: 'both',
        bold: true,
        spacingBefore: 120,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: 'Thực hiện theo quy định của Bộ luật Lao động và nội quy lao động của Bên A.',
        align: 'both',
        indentFirstLine: INDENT_FIRST,
        spacingAfter: 180,
        lineSpacing: LINE_15,
      },
      {
        text: 'Điều 6. Điều khoản thi hành',
        align: 'both',
        bold: true,
        spacingBefore: 120,
        spacingAfter: 80,
        lineSpacing: LINE_15,
      },
      {
        text: 'Hợp đồng có hiệu lực kể từ ngày ký. Lập thành 02 bản, mỗi bên giữ 01 bản có giá trị pháp lý như nhau.',
        align: 'both',
        indentFirstLine: INDENT_FIRST,
        spacingAfter: 360,
        lineSpacing: LINE_15,
      },
      {
        text: '{{signPlace}}, ngày {{signDate}}',
        align: 'right',
        italic: true,
        spacingAfter: 360,
        lineSpacing: LINE_15,
      },
      {
        type: 'table',
        rows: [
          {
            left: 'ĐẠI DIỆN BÊN A',
            right: 'NGƯỜI LAO ĐỘNG',
            bold: true,
            spacingBefore: 120,
            spacingAfter: 80,
          },
          {
            left: '(Ký, ghi rõ họ tên)',
            right: '(Ký, ghi rõ họ tên)',
            italic: true,
            spacingAfter: 240,
          },
          {
            left: '{{employerRepresentative}}',
            right: '{{fullName}}',
            bold: true,
            spacingAfter: 120,
          },
        ],
      },
    ],
  },
]

export function createSampleDocxBlob(definition: SampleDocxDefinition): Blob {
  return createDocxFromBlocks(definition.blocks)
}
