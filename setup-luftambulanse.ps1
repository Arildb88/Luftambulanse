# ==========================================================
# Luftambulanse - Automated setup and launch (PowerShell)
# ==========================================================

# ---------- [0] Relaunch elevated with ExecutionPolicy Bypass if needed ----------
$needAdmin  = -not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()
                ).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
$procPolicy = Get-ExecutionPolicy -Scope Process
$needBypass = $procPolicy -ne 'Bypass'

if ($needAdmin -or $needBypass) {
    $args = @(
        '-NoProfile',
        '-ExecutionPolicy','Bypass',
        '-File', "`"$PSCommandPath`""
    )
    Start-Process powershell.exe -Verb RunAs -ArgumentList $args
    exit
}

Write-Host "==================================================="
Write-Host "Luftambulanse - Automatic environment setup"
Write-Host "===================================================`n"

# ---------- [1] Basics ----------
Set-Location $PSScriptRoot

# Paths and settings (adjust if you rename things)
$ComposeFile  = Join-Path $PSScriptRoot 'docker-compose.yml'
$ProjectPath  = Join-Path $PSScriptRoot 'Project\Gruppe4NLA.csproj'
$StartupPath  = $ProjectPath

# DB host/port as exposed by docker-compose (ports: "3306:3306")
$DbHost = '127.0.0.1'
$DbPort = 3306

# # ---------- [2] Helpers ----------
# function Wait-For-Docker {
#     Write-Host "Waiting for Docker engine..." -ForegroundColor Cyan
#     $deadline = (Get-Date).AddMinutes(3)
#     while ((Get-Date) -lt $deadline) {
#         try {
#             & docker info *> $null
#             if ($LASTEXITCODE -eq 0) {
#                 Write-Host "Docker is ready." -ForegroundColor Green
#                 return
#             }
#         } catch { }
#         Start-Sleep -Seconds 3
#     }
#     throw "Docker did not become ready in time."
# }

# function Wait-For-Port {
#     param(
#         [string]$Host,
#         [int]$Port,
#         [int]$TimeoutSeconds = 120
#     )
#     Write-Host "Waiting for TCP $Host:$Port ..." -ForegroundColor Cyan
#     $sw = [Diagnostics.Stopwatch]::StartNew()
#     while ($sw.Elapsed.TotalSeconds -lt $TimeoutSeconds) {
#         try {
#             $tcp = New-Object System.Net.Sockets.TcpClient
#             $ar  = $tcp.BeginConnect($Host, $Port, $null, $null)
#             if ($ar.AsyncWaitHandle.WaitOne(1500)) {
#                 $tcp.EndConnect($ar)
#                 $tcp.Close()
#                 Write-Host "Port $Host:$Port is accepting connections." -ForegroundColor Green
#                 return
#             }
#             $tcp.Close()
#         } catch { }
#         Start-Sleep -Milliseconds 800
#     }
#     throw "Timeout waiting for $Host:$Port."
# }

# # ---------- [3] Docker Desktop check/start ----------
# Write-Host "Checking for Docker Desktop..." -ForegroundColor Cyan
# $dockerPath = Get-Command docker -ErrorAction SilentlyContinue
# if (-not $dockerPath) {
#     Write-Host "Docker CLI not found. Install Docker Desktop first: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
#     Write-Host "You can re-run this script after installing Docker Desktop." -ForegroundColor Yellow
#     Pause
#     exit 1
# }

# # Try to start Docker Desktop if not already running (best effort)
# try { Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe" -ErrorAction SilentlyContinue | Out-Null } catch { }

# # Wait for Docker engine
# try { Wait-For-Docker } catch { Write-Host "Warning: $_" -ForegroundColor Yellow }

# ---------- [4] Start containers BEFORE EF (DB must be up) ----------
Write-Host "`nStarting docker compose..." -ForegroundColor Cyan
if (Test-Path $ComposeFile) {
    & docker compose -f "$ComposeFile" up -d
} else {
    & docker compose up -d
}
if ($LASTEXITCODE -ne 0) {
    Write-Host "docker compose failed. Please ensure Docker Desktop is running." -ForegroundColor Red
    Pause
    exit 1
}

# Wait for DB port to be reachable
try { Wait-For-Port -Host $DbHost -Port $DbPort -TimeoutSeconds 120 } catch { Write-Host "Warning: $_" -ForegroundColor Yellow }

# ---------- [5] Stop local MariaDB/MySQL (avoid port conflicts) ----------
Write-Host "`nStopping local MariaDB/MySQL services (if running)..." -ForegroundColor Cyan
net stop "mariadb" >$null 2>&1
net stop "mysql"   >$null 2>&1
Write-Host "Services stopped (or not running)."

# ---------- [6] .NET SDK and EF Core CLI ----------
Write-Host "`nChecking .NET SDK..." -ForegroundColor Cyan
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "The .NET SDK was not found. Install from https://dotnet.microsoft.com/download/dotnet" -ForegroundColor Red
    Pause
    exit 1
}
Write-Host ".NET SDK found."

Write-Host "`nChecking EF Core CLI..." -ForegroundColor Cyan
$efList = dotnet tool list -g 2>$null
$hasEf = $false
if ($efList) { $hasEf = $efList -match 'dotnet-ef' }
if (-not $hasEf) {
    Write-Host "Installing EF Core CLI..."
    dotnet tool install --global dotnet-ef | Out-Null
    $efList = dotnet tool list -g 2>$null
    if ($efList -notmatch 'dotnet-ef') {
        Write-Host "Failed to install EF Core CLI." -ForegroundColor Red
        Pause
        exit 1
    }
} else {
    Write-Host "EF Core CLI already installed."
}

# ---------- [7] Restore and run migrations ----------
if (-not (Test-Path $ProjectPath)) {
    Write-Host "Project file not found: $ProjectPath" -ForegroundColor Red
    Write-Host "Check the path in the script matches your repo layout." -ForegroundColor Yellow
    Pause
    exit 1
}

Write-Host "`nRestoring NuGet packages..." -ForegroundColor Cyan
dotnet restore "$ProjectPath"
if ($LASTEXITCODE -ne 0) {
    Write-Host "dotnet restore failed." -ForegroundColor Red
    Pause
    exit 1
}

Write-Host "`nUpdating database via EF migrations..." -ForegroundColor Cyan
dotnet ef database update --project "$ProjectPath" --startup-project "$StartupPath"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Database update failed." -ForegroundColor Red
    Pause
    exit 1
}
Write-Host "Database updated." -ForegroundColor Green

# ---------- [8] Launch the app ----------
Write-Host "`nLaunching ASP.NET Core app in a new window..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit","-Command","dotnet watch run --project `"$StartupPath`""

Write-Host "`nDone! If the browser does not open automatically, go to http://localhost:5000" -ForegroundColor Green
Pause
