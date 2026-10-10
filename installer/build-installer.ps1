<#
.SYNOPSIS
    Kurulum paketini (setup.exe) bastan sona uretir.

.DESCRIPTION
    1. 'publish.ps1' ile WinForms istemcisini ve caa-provision aracini Release
       olarak yayinlar (sifir geldiyse, -p:Version ile assembly surumu set edilir).
    2. Inno Setup derleyicisi (ISCC.exe) ile 'CostAccountingAutomation.iss'
       betigini derleyip 'installer\dist' altina setup.exe uretir.

    Version parametresi verilmezse veritabanindaki (AppReleases) son surumden
    otomatik +1 hesaplanir: DB '1.0.0.1' ise yeni surum '1.0.0.2' derlenir.
    Bunun icin gecici olarak yalnizca caa-provision aracini yayinlar ve
    'latest-version' komutuyla en son surumu okur.

    Inno Setup kurulu degilse:
        winget install --id JRSoftware.InnoSetup -e

.PARAMETER Version
    Setup/assembly surumu (4 haneli). Verilmezse veritabanindaki son surum +1.
    Ornek: 1.0.0.2

.PARAMETER Iscc
    ISCC.exe tam yolu. Verilmezse yaygin konumlar ve PATH aranir.

.EXAMPLE
    .\build-installer.ps1
    .\build-installer.ps1 -Version 1.1.0.0
#>
[CmdletBinding()]
param(
    [string]$Version = '',
    [string]$Iscc = ''
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$srcRoot = Join-Path $repoRoot 'src'
$provProject = Join-Path $srcRoot 'Cost.Accounting.Automation.Provisioning\Cost.Accounting.Automation.Provisioning.csproj'

function Resolve-Iscc([string]$Explicit) {
    if ($Explicit -and (Test-Path -LiteralPath $Explicit)) {
        return $Explicit
    }

    $candidates = @(
        (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
        (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe'),
        (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe')
    )

    foreach ($candidate in $candidates) {
        if ($candidate -and (Test-Path -LiteralPath $candidate)) {
            return $candidate
        }
    }

    $cmd = Get-Command iscc.exe -ErrorAction SilentlyContinue
    if ($cmd) {
        return $cmd.Source
    }

    throw 'ISCC.exe (Inno Setup) bulunamadi. Kurmak icin: winget install --id JRSoftware.InnoSetup -e'
}

function Get-NextVersion([string]$Latest) {
    if (-not $Latest) {
        return '1.0.0.1'
    }

    $parts = $Latest -split '\.'
    while ($parts.Count -lt 4) {
        $parts += '0'
    }

    try {
        $next = [int]$parts[3] + 1
        if ($next -gt 9) {
            $parts[2] = [string]([int]$parts[2] + 1)
            $next = 0
        }
        $parts[3] = [string]$next
    }
    catch {
        throw "Veritabanindaki surum artirilamadi: '$Latest'"
    }

    return ($parts -join '.')
}

# --- 1) Surum belirlenir --------------------------------------------------
if (-not $Version) {
    Write-Host 'Adim 0/3 - Veritabanindaki son surum okunuyor (latest-version)...' -ForegroundColor Cyan
    $tmpProv = Join-Path $env:TEMP 'caa-provision-versioncheck'
    if (Test-Path -LiteralPath $tmpProv) {
        Remove-Item -LiteralPath $tmpProv -Recurse -Force
    }

    & dotnet publish $provProject -c Release -r win-x64 --self-contained true -o $tmpProv
    if (-not $?) { throw 'Surum sorgusu icin caa-provision yayinlanamadi.' }

    $provExe = Join-Path $tmpProv 'caa-provision.exe'
    $latest = (& $provExe latest-version).Trim()
    if ($LASTEXITCODE -ne 0) { throw "latest-version komutu basarisiz oldu (kod $LASTEXITCODE)." }

    $Version = Get-NextVersion $latest
    Write-Host "DB'deki son surum: '$latest'"
    Write-Host "Derlenecek yeni surum: $Version"
}

# --- 2) Yayinlama --------------------------------------------------------
Write-Host ''
Write-Host 'Adim 1/3 - Yayinlama (publish)...' -ForegroundColor Cyan
& (Join-Path $PSScriptRoot 'publish.ps1') -Configuration Release -Version $Version
if (-not $?) { throw 'Yayinlama basarisiz oldu.' }

# --- 3) Inno Setup derlemesi ---------------------------------------------
Write-Host ''
Write-Host 'Adim 2/3 - Setup derleniyor...' -ForegroundColor Cyan

$isccPath = Resolve-Iscc $Iscc
$issPath = Join-Path $PSScriptRoot 'CostAccountingAutomation.iss'

Write-Host "ISCC: $isccPath"
& $isccPath "/DAppVersion=$Version" "/DStagingDir=staging" $issPath
if (-not $?) { throw 'Inno Setup derlemesi basarisiz oldu.' }

$output = Join-Path $PSScriptRoot "dist\CostAccountingAutomation-Setup-$Version.exe"
Write-Host ''
Write-Host "Kurulum paketi hazir: $output" -ForegroundColor Green
Write-Host ''
Write-Host "Bir sonraki adim: yayini veritabanina yazmak" -ForegroundColor Cyan
Write-Host "  .\publish-update.ps1 -Version $Version -SetupPath (Join-Path $PSScriptRoot \"dist\CostAccountingAutomation-Setup-$Version.exe\")" -ForegroundColor Green