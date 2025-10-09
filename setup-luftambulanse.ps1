# ==========================================================
# 🚁 Luftambulanse - Automated Setup and Launch Script
# ==========================================================
# This script:
# ✅ Ensures admin privileges
# ✅ Safely requests temporary script permission
# ✅ Installs Docker Desktop if missing
# ✅ Stops local MariaDB/MySQL
# ✅ Updates EF Core database
# ✅ Starts Docker containers
# ✅ Launches the ASP.NET Core app
# ==========================================================

# --- Check for admin privileges ---
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "🔒 Restarting PowerShell as Administrator..."
    Start-Process powershell -Verb runAs -ArgumentList "-File `"$PSCommandPath`""
    exit
}

Write-Host "==================================================="
Write-Host "🚁 Luftambulanse - Automatic Environment Setup"
Write-Host "===================================================`n"

# --- Ask user to allow temporary script execution if restricted ---
$policy = Get-ExecutionPolicy -Scope Process
if ($policy -eq 'Restricted') {
    $response = Read-Host "⚠️ PowerShell scripts are currently restricted. Temporarily allow this script to run? (y/n)"
    if ($response -eq 'y') {
        try {
            Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force
            Write-Host "✅ Script execution temporarily allowed for this session." -ForegroundColor Green
        } catch {
            Write-Host "❌ Could not modify execution policy. Try running as Administrator." -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "❌ Aborted by user. Script cannot continue." -ForegroundColor Red
        exit 1
    }
}

# --- Navigate to script location ---
Set-Location $PSScriptRoot

# --- Check for Docker Desktop ---
Write-Host "`n🔍 Checking for Docker Desktop..." -ForegroundColor Cyan
$dockerPath = (Get-Command docker -ErrorAction SilentlyContinue)

if (-not $dockerPath) {
    Write-Host "🐳 Docker Desktop not found. Installing..." -ForegroundColor Yellow
    $dockerInstaller = "docker-desktop-installer.exe"
    $dockerUrl = "https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe"

    if (-not (Test-Path $dockerInstaller)) {
        Write-Host "⬇️ Downloading Docker Desktop installer..."
        Invoke-WebRequest -UseBasicParsing -OutFile $dockerInstaller -Uri $dockerUrl
    }

    Write-Host "⚙️ Installing Docker Desktop (this may take several minutes)..."
    Start-Process -FilePath .\docker-desktop-
