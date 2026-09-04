# Recursively delete bin, obj, and node_modules under a root path.
# Usage:
#   .\clean.ps1
#   .\clean.ps1 -Path D:\Working\Personal\jarvis
#   .\clean.ps1 -WhatIf

[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter()]
    [string]$Path = $PSScriptRoot
)

$ErrorActionPreference = "Stop"
$targets = @("bin", "obj", "node_modules")

if (-not (Test-Path -LiteralPath $Path)) {
    Write-Error "Path not found: $Path"
    exit 1
}

$root = (Resolve-Path -LiteralPath $Path).Path
Write-Host "Scanning: $root"
Write-Host "Removing: $($targets -join ', ')"
Write-Host ""

$folders = Get-ChildItem -LiteralPath $root -Directory -Recurse -Force -ErrorAction SilentlyContinue |
    Where-Object { $targets -contains $_.Name }

if (-not $folders) {
    Write-Host "Nothing to clean."
    exit 0
}

$removed = 0
$failed = 0

foreach ($folder in $folders) {
    # Skip nested matches already under a deleted parent (e.g. node_modules/**/node_modules)
    if (-not (Test-Path -LiteralPath $folder.FullName)) {
        continue
    }

    if ($PSCmdlet.ShouldProcess($folder.FullName, "Remove directory")) {
        try {
            Remove-Item -LiteralPath $folder.FullName -Recurse -Force -ErrorAction Stop
            Write-Host "Removed: $($folder.FullName)"
            $removed++
        }
        catch {
            Write-Warning "Failed: $($folder.FullName) - $($_.Exception.Message)"
            $failed++
        }
    }
}

Write-Host ""
Write-Host "Done. Removed $removed folder(s). Failed: $failed."
if ($failed -gt 0) { exit 1 }
