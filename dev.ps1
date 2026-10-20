#Requires -Version 5.1
<#
    Starts the TimeIsNow API + Vite client in the background, then opens the browser.

    Usage:
        .\dev.ps1                start everything (reuses anything already running)
        .\dev.ps1 -NoBrowser     start without opening the browser
        .\dev.ps1 -Rebuild       force a dotnet build before starting the API
#>
[CmdletBinding()]
param(
    [switch]$NoBrowser,
    [switch]$Rebuild
)

$ErrorActionPreference = 'Stop'

$root       = $PSScriptRoot
$apiProject = Join-Path $root 'src\TimeIsNow.API'
$apiBin     = Join-Path $apiProject 'bin\Debug\net10.0'
$clientDir  = Join-Path $root 'client'
$runDir     = Join-Path $root '.run'
$logDir     = Join-Path $root 'logs'

$apiUrl     = 'http://localhost:5099'
$clientUrl  = 'http://localhost:5173'
$apiPort    = 5099
$clientPort = 5173

New-Item -ItemType Directory -Force -Path $runDir, $logDir | Out-Null

function Test-Port {
    param([int]$Port)
    foreach ($address in @('127.0.0.1', '::1')) {
        $client = New-Object System.Net.Sockets.TcpClient
        try {
            $task = $client.ConnectAsync($address, $Port)
            if ($task.Wait(400) -and $client.Connected) { return $true }
        } catch {
            # not listening on this address family
        } finally {
            $client.Dispose()
        }
    }
    return $false
}

function Wait-Port {
    param([int]$Port, [int]$TimeoutSec)
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    while ((Get-Date) -lt $deadline) {
        if (Test-Port $Port) { return $true }
        Start-Sleep -Milliseconds 300
    }
    return $false
}

# ---- API ----
if (Test-Port $apiPort) {
    Write-Host '  API     already running  ' -NoNewline -ForegroundColor DarkGray
    Write-Host $apiUrl -ForegroundColor DarkGray
} else {
    $dll = Join-Path $apiBin 'TimeIsNow.API.dll'
    if ($Rebuild -or -not (Test-Path -LiteralPath $dll)) {
        Write-Host '  API     building...' -ForegroundColor Yellow
        & dotnet build $apiProject -v quiet
        if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed.' }
    }
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $api = Start-Process -FilePath 'dotnet' `
        -ArgumentList @('TimeIsNow.API.dll', '--urls', $apiUrl) `
        -WorkingDirectory $apiBin -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput (Join-Path $logDir 'api.log') `
        -RedirectStandardError  (Join-Path $logDir 'api.err.log')
    $api.Id | Set-Content -LiteralPath (Join-Path $runDir 'api.pid')
    Write-Host '  API     starting         ' -NoNewline -ForegroundColor Cyan
    Write-Host $apiUrl -ForegroundColor Cyan
}

# ---- Client ----
if (Test-Port $clientPort) {
    Write-Host '  Client  already running  ' -NoNewline -ForegroundColor DarkGray
    Write-Host $clientUrl -ForegroundColor DarkGray
} else {
    $vite = Start-Process -FilePath 'cmd.exe' `
        -ArgumentList @('/c', 'npm run dev') `
        -WorkingDirectory $clientDir -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput (Join-Path $logDir 'vite.log') `
        -RedirectStandardError  (Join-Path $logDir 'vite.err.log')
    $vite.Id | Set-Content -LiteralPath (Join-Path $runDir 'vite.pid')
    Write-Host '  Client  starting         ' -NoNewline -ForegroundColor Cyan
    Write-Host $clientUrl -ForegroundColor Cyan
}

$apiReady    = Wait-Port $apiPort 30
$clientReady = Wait-Port $clientPort 60

if (-not $apiReady)    { Write-Warning "API did not answer on $apiUrl. See $logDir\api.log" }
if (-not $clientReady) { Write-Warning "Client did not answer on $clientUrl. See $logDir\vite.log" }

if ($clientReady -and -not $NoBrowser) {
    Start-Process $clientUrl
}

if ($apiReady -and $clientReady) {
    Write-Host '  Ready   -> ' -NoNewline -ForegroundColor Green
    Write-Host $clientUrl -ForegroundColor Green
}
