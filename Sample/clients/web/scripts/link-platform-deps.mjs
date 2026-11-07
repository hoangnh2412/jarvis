import fs from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const webRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..')
const nodeModules = path.join(webRoot, 'node_modules')

/** npm không hoist dependency của package `file:` lên host. Junction để Vite thấy một bản tại root và pre-bundle. */
const links = {
  'axios': '../../../frameworks/frontend/node_modules/axios',
  'lucide-react': '../../../frameworks/frontend/node_modules/lucide-react',
  'primereact': '../../../frameworks/frontend/node_modules/primereact',
  'flatpickr': '../../../modules/settings/frontend/node_modules/flatpickr',
  '@microsoft/fetch-event-source':
    '../../../modules/notifications/frontend/node_modules/@microsoft/fetch-event-source',
  '@atlaskit/pragmatic-drag-and-drop':
    '../../../frameworks/frontend/node_modules/@atlaskit/pragmatic-drag-and-drop',
  '@hookform/resolvers': '../../../frameworks/frontend/node_modules/@hookform/resolvers',
}

for (const [name, rel] of Object.entries(links)) {
  const target = path.resolve(webRoot, rel)
  const dest = path.join(nodeModules, name)
  if (!fs.existsSync(target)) {
    console.warn(`link-platform-deps: missing ${target}`)
    continue
  }
  fs.mkdirSync(path.dirname(dest), { recursive: true })
  fs.rmSync(dest, { recursive: true, force: true })
  fs.symlinkSync(target, dest, 'junction')
}
