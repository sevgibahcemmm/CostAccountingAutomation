<#
.SYNOPSIS
    Kurulum paketini (setup.exe) bastan sona uretir.

.DESCRIPTION
    1. 'publish.ps1' ile WinForms istemcisini ve caa-provision aracini Release
       olarak yayinlar.
    2. Inno Setup derleyicisi (ISCC.exe) ile 'CostAccountingAutomation.iss'
       betigini derleyip 'installer\dist' altina setup.exe uretir.

    Inno Setup kurulu degilse:
        winget install --id JRSoftware.InnoSetup -e

.PARAMETER Version
    Setup surumu. Ornek: 1.0.0, 1.2.3

.PARAMETER Iscc
    ISCC.exe tam yolu. Verilmezse yaygin konumlar ve PATH aranir.

.PARAMETER UpdateUrl
    Verilirse repo kokundeki update.json, bu surum ve indirme adresiyle
    guncellenir (uygulama acilista bu dosyayi okuyup guncelleme bildirir).
    Bos birakilirsa update.json'a dokunulmaz.

.EXAMPLE
    .\build-installer.ps1
    .\build-installer.ps1 -Version 1.1.0
    .\build-installer.ps1 -Version 1.1.0 -UpdateUrl "https://.../Setup-1.1.0.exe"
#>
[CmdletBinding()]
param(
    [string]$Version = '1.0.0',
    [string]$Iscc = '',
    [string]$UpdateUrl = ''
)

$ErrorActionPreference = 'Stop'

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

Write-Host 'Adim 1/2 - Yayinlama (publish)...' -ForegroundColor Cyan
& (Join-Path $PSScriptRoot 'publish.ps1') -Configuration Release

Write-Host ''
Write-Host 'Adim 2/2 - Setup derleniyor...' -ForegroundColor Cyan

$isccPath = Resolve-Iscc $Iscc
$issPath = Join-Path $PSScriptRoot 'CostAccountingAutomation.iss'

Write-Host "ISCC: $isccPath"
& $isccPath "/DAppVersion=$Version" "/DStagingDir=staging" $issPath
if (-not $?) { throw 'Inno Setup derlemesi basarisiz oldu.' }

$output = Join-Path $PSScriptRoot "dist\CostAccountingAutomation-Setup-$Version.exe"
Write-Host ''
Write-Host "Kurulum paketi hazir: $output" -ForegroundColor Green

if ($UpdateUrl) {
    $repoRoot = Split-Path -Parent $PSScriptRoot
    $manifestPath = Join-Path $repoRoot 'update.json'
    $manifest = [ordered]@{
        version   = $Version
        url       = $UpdateUrl
        notes     = "Surum $Version"
        mandatory = $false
    }
    $manifest | ConvertTo-Json | Set-Content -LiteralPath $manifestPath -Encoding UTF8
    Write-Host "update.json guncellendi: $manifestPath" -ForegroundColor Green
    Write-Host "NOT: update.json yalnizca gecmis kayit icindir; istemciler surum kaydini Master veritabanindaki AppReleases tablosundan okur (sunucudaki sync-updates.ps1 tabloyu da yazar)." -ForegroundColor Yellow
}