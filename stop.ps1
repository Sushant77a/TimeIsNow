#Requires -Version 5.1
<#
    Stops the TimeIsNow API + Vite client started by dev.ps1.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'SilentlyContinue'

$root       = $PSScriptRoot
$runDir     = Join-Path $root '.run'
$apiPort    = 5099
$clientPort = 5173

function Stop-Tree {
    param([int]$ProcessId, [string]$Label)
    taskkill /PID $ProcessId /T /F *> $null
    if ($LASTEXITCODE -eq 0) {
        Write-Host '  ' -NoNewline
        Write-Host $Label.PadRight(8) -NoNewline -ForegroundColor Green
        Write-Host "stopped (pid $ProcessId)" -ForegroundColor Gray
    }
}

# Primary: the PIDs captured when dev.ps1 launched the processes.
foreach ($name in @('api', 'vite')) {
    $pidFile = Join-Path $runDir "$name.pid"
    if (Test-Path -LiteralPath $pidFile) {
        $processId = (Get-Content -LiteralPath $pidFile -First 1).Trim()
        if ($processId -match '^\d+$') { Stop-Tree -ProcessId ([int]$processId) -Label $name }
        Remove-Item -LiteralPath $pidFile -Force
    }
}

# Fallback: whoever owns the ports (covers processes started outside dev.ps1).
function Stop-Port {
    param([int]$Port, [string]$Label)
    if (-not (Get-Command Get-NetTCPConnection -ErrorAction SilentlyContinue)) { return }
    foreach ($conn in (Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue)) {
        Stop-Tree -ProcessId $conn.OwningProcess -Label $Label
    }
}

Stop-Port -Port $apiPort    -Label 'API'
Stop-Port -Port $clientPort -Label 'Client'

Write-Host '  Done.' -ForegroundColor DarkGray
