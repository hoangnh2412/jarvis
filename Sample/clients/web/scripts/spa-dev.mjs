import { execSync, spawn } from 'node:child_process'
import process from 'node:process'

const port = Number(process.env.VITE_PORT || 5173)

function freePort(portNumber) {
  try {
    if (process.platform === 'win32') {
      execSync(
        `powershell -NoProfile -Command "Get-NetTCPConnection -LocalPort ${portNumber} -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }"`,
        { stdio: 'ignore' },
      )
    } else {
      execSync(`fuser -k ${portNumber}/tcp`, { stdio: 'ignore' })
    }
  } catch {
    // Port already free — continue.
  }
}

freePort(port)

const child = spawn('npx', ['vite', '--host', '127.0.0.1', '--port', String(port)], {
  stdio: 'inherit',
  shell: true,
  env: process.env,
})

child.on('exit', (code, signal) => {
  if (signal) {
    process.kill(process.pid, signal)
    return
  }
  process.exit(code ?? 1)
})
