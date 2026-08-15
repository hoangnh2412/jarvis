import { existsSync, readFileSync } from 'node:fs'
import { spawnSync } from 'node:child_process'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..')
const forceBuild = process.argv.includes('--force-build')

function readPkg(dir) {
  const p = path.join(dir, 'package.json')
  if (!existsSync(p)) return null
  return JSON.parse(readFileSync(p, 'utf8'))
}

function fileDepDirs(pkgDir, pkg) {
  const sections = [pkg.dependencies, pkg.devDependencies, pkg.optionalDependencies, pkg.peerDependencies]
  const dirs = []
  for (const section of sections) {
    if (!section) continue
    for (const spec of Object.values(section)) {
      if (typeof spec !== 'string' || !spec.startsWith('file:')) continue
      dirs.push(path.resolve(pkgDir, spec.slice('file:'.length)))
    }
  }
  return dirs
}

/** Collect host `file:` packages + their transitive local `file:` deps. */
function collectLinkedGraph() {
  const host = readPkg(root)
  if (!host) {
    console.error('[ensure-linked-deps] missing host package.json')
    process.exit(1)
  }

  /** @type {Map<string, { dir: string, pkg: object, deps: string[] }>} */
  const nodes = new Map()
  const queue = fileDepDirs(root, host)

  while (queue.length) {
    const dir = queue.shift()
    const key = path.normalize(dir)
    if (nodes.has(key)) continue

    const pkg = readPkg(dir)
    if (!pkg) {
      console.warn(`[ensure-linked-deps] missing package.json: ${path.relative(root, dir)}`)
      continue
    }

    const depDirs = fileDepDirs(dir, pkg).map((d) => path.normalize(d))
    nodes.set(key, { dir, pkg, deps: depDirs })
    for (const d of depDirs) {
      if (!nodes.has(d)) queue.push(d)
    }
  }

  return nodes
}

/** Kahn topological sort. Edges: dep → package (build deps first). */
function topoSort(nodes) {
  const indegree = new Map()
  const dependents = new Map()

  for (const key of nodes.keys()) {
    indegree.set(key, 0)
    dependents.set(key, [])
  }

  for (const [key, node] of nodes) {
    for (const dep of node.deps) {
      if (!nodes.has(dep)) continue
      indegree.set(key, (indegree.get(key) ?? 0) + 1)
      dependents.get(dep).push(key)
    }
  }

  const queue = [...indegree.entries()].filter(([, n]) => n === 0).map(([k]) => k)
  const ordered = []

  while (queue.length) {
    const key = queue.shift()
    ordered.push(nodes.get(key))
    for (const next of dependents.get(key) ?? []) {
      const n = indegree.get(next) - 1
      indegree.set(next, n)
      if (n === 0) queue.push(next)
    }
  }

  if (ordered.length !== nodes.size) {
    const left = [...nodes.keys()].filter((k) => !ordered.some((n) => path.normalize(n.dir) === k))
    console.error('[ensure-linked-deps] cycle in file: dependencies:')
    for (const k of left) console.error(`  - ${path.relative(root, k)}`)
    process.exit(1)
  }

  return ordered
}

function runNpm(cwd, args, label) {
  console.log(`[ensure-linked-deps] npm ${args.join(' ')} → ${label}`)
  const r = spawnSync('npm', args, { cwd, stdio: 'inherit', shell: true })
  if (r.status !== 0) process.exit(r.status ?? 1)
}

function npmInstall(cwd) {
  const label = path.relative(root, cwd) || '.'
  if (!existsSync(path.join(cwd, 'package.json'))) {
    console.warn(`[ensure-linked-deps] missing package.json: ${label}`)
    return
  }
  if (existsSync(path.join(cwd, 'node_modules'))) {
    console.log(`[ensure-linked-deps] skip install (exists): ${label}`)
    return
  }
  runNpm(cwd, ['install'], label)
}

function resolveExportTypes(pkg) {
  const exp = pkg.exports?.['.']
  if (typeof exp === 'string') return exp
  if (exp && typeof exp === 'object') return exp.types || exp.import || exp.require || exp.default
  return null
}

function buildOutputReady(pkg, dir) {
  const types = pkg.types || (typeof pkg.exports?.['.'] === 'object' ? pkg.exports['.'].types : null)
  if (types) return existsSync(path.join(dir, types))

  const candidates = [resolveExportTypes(pkg), pkg.module, pkg.main].filter(Boolean)
  if (candidates.length === 0) return existsSync(path.join(dir, 'dist'))
  return candidates.some((rel) => existsSync(path.join(dir, rel)))
}

function needsBuild(pkg, dir) {
  if (!pkg.scripts?.build) return false
  if (forceBuild) return true
  // Skip only when declared entry (types/main/module) already exists.
  return !buildOutputReady(pkg, dir)
}

function npmBuild(cwd, pkg) {
  const label = path.relative(root, cwd) || '.'
  if (!needsBuild(pkg, cwd)) {
    if (pkg.scripts?.build) {
      console.log(`[ensure-linked-deps] skip build (dist exists): ${label}`)
    }
    return
  }
  runNpm(cwd, ['run', 'build'], label)
}

const nodes = collectLinkedGraph()
const ordered = topoSort(nodes)

console.log(
  '[ensure-linked-deps] order:',
  ordered.map((n) => n.pkg.name || path.relative(root, n.dir)).join(' → ') || '(none)',
)

for (const node of ordered) {
  npmInstall(node.dir)
}
for (const node of ordered) {
  npmBuild(node.dir, node.pkg)
}

npmInstall(root)
