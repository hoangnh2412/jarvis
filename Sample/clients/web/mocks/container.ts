import path from 'node:path'
import type { Plugin } from 'vite'
import { craftDocApiMockPlugin } from '../vite.plugins/craftDocApiMock.js'
import { plannerApiMockPlugin } from '../vite.plugins/plannerApiMock.js'
import { timesheetApiMockPlugin } from '../vite.plugins/timesheetApiMock.js'

const mockFiles = {
  planner: { boardJsonPath: 'planner/get-board.json' },
  timesheet: {
    boardJsonPath: 'timesheet/get-board.json',
    optionsJsonPath: 'timesheet/get-options.json',
  },
  craftDoc: {
    templatesListJsonPath: 'craftDoc/get-templates-list.json',
    sampleBriefTemplateJsonPath: 'craftDoc/get-sample-brief-template.json',
    sampleBriefDocxPath: 'craftDoc/assets/Sample-Brief.docx',
  },
} as const

export function sampleMockPlugins(hostRoot: string): Plugin[] {
  const mocksRoot = path.join(hostRoot, 'mocks')
  const file = (relativePath: string) => path.join(mocksRoot, relativePath)

  return [
    plannerApiMockPlugin({
      boardJsonPath: file(mockFiles.planner.boardJsonPath),
      enabled: true,
    }),
    timesheetApiMockPlugin({
      boardJsonPath: file(mockFiles.timesheet.boardJsonPath),
      optionsJsonPath: file(mockFiles.timesheet.optionsJsonPath),
      enabled: true,
    }),
    craftDocApiMockPlugin({
      enabled: true,
      templatesListJsonPath: file(mockFiles.craftDoc.templatesListJsonPath),
      sampleBriefTemplateJsonPath: file(mockFiles.craftDoc.sampleBriefTemplateJsonPath),
      sampleBriefDocxPath: file(mockFiles.craftDoc.sampleBriefDocxPath),
    }),
  ]
}
