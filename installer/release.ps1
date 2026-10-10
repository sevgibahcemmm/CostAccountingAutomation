<#
.SYNOPSIS
    Tek komutla: surumu artir, setup.exe uret ve veritabanina guncelleme olarak yayinla.

.DESCRIPTION
    Uc adimi otomatik yapar:

    1. Merkezi veritabanindaki (AppReleases) son surumu okur ve son haneyi +1
       artirir. -Version verilirse o surum kullanilir.
    2. build-installer.ps1 ile kurulum paketini (setup.exe) uretir.
    3. publish-update.ps1 ile setup dosyasini yeni surum olarak veritabanina
       yazar. Kurulu istemciler acilista bu surumu gorup kendilerini gunceller;
       harici sunucu/URL gerekmez.

    NOT: Bu dosya bilerek yalnizca ASCII karakter icerir. Windows PowerShell 5.1,
    BOM'suz UTF-8 dosyalari ANSI kodlamasiyla okur; Turkce karakterler iceren
    yorumlar satirlari bozup ayristirma hatasi uretir.

.PARAMETER Version
    Yayinlanacak surum (4 haneli). Verilmezse veritabanindaki son surum +1.

.PARAMETER Iscc
    ISCC.exe tam yolu. Verilmezse build-installer.ps1 yaygin konumlari arar.

.PARAMETER Notes
    Kullanicilara gosterilecek surum notlari. Varsayilan: "Surum x.y.z".

.PARAMETER Mandatory
    Zorunlu guncelleme: kullanici guncellemeyi atlayamaz, kapatamaz.

.EXAMPLE
    .\release.ps1
    .\release.ps1 -Version 1.1.0.0 -Notes "Yeni modul ve duzeltmeler"
    .\release.ps1 -Notes "Guvenlik duzeltmesi" -Mandatory
#>
[CmdletBinding()]
param(
    [string]$Version = '',
    [string]$Iscc = '',
    [string]$Notes = '',
    [switch]$Mandatory
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$srcRoot = Join-Path $repoRoot 'src'
$provProject = Join-Path $srcRoot 'Cost.Accounting.Automation.Provisioning\Cost.Accounting.Automation.Provisioning.csproj'

function Get-NextVersion([string]$Latest) {
    if (-not $Latest) {
        return '1.0.0.1'
    }

    $parts = $Latest -split '\.'
    while ($parts.Count -lt 4) {
        $parts += '0'
    }

    try {
        $parts[3] = [int]$parts[3] + 1
    }
    catch {
        throw "Veritabanindaki surum artirilamadi: '$Latest'"
    }

    return ($parts -join '.')
}

# --- 1) Surum belirlenir --------------------------------------------------
if (-not $Version) {
    Write-Host 'Surum sorgusu: veritabanindaki son surum okunuyor...' -ForegroundColor Cyan
    $tmpProv = Join-Path $env:TEMP 'caa-provision-releasecheck'
    if (Test-Path -LiteralPath $tmpProv) {
        Remove-Item -LiteralPath $tmpProv -Recurse -Force
    }

    & dotnet publish $provProject -c Release -r win-x64 --self-contained true -o $tmpProv
    if (-not $?) { throw 'Surum sorgusu icin caa-provision yayinlanamadi.' }

    $provExe = Join-Path $tmpProv 'caa-provision.exe'
    $latest = (& $provExe latest-version).Trim()
    if ($LASTEXITCODE -ne 0) { throw "latest-version komutu basarisiz oldu (kod $LASTEXITCODE)." }

    $Version = Get-NextVersion $latest
    Write-Host "DB'deki son surum: '$latest' -> yeni surum: $Version" -ForegroundColor Green
}

# --- 2) Setup uretilir ----------------------------------------------------
Write-Host ''
Write-Host "Adim 1/2 - Kurulum paketi uretiliyor ($Version)..." -ForegroundColor Cyan
$buildArgs = @('-Version', $Version)
if ($Iscc) {
    $buildArgs += @('-Iscc', $Iscc)
}

& (Join-Path $PSScriptRoot 'build-installer.ps1') @buildArgs
if (-not $?) { throw 'build-installer basarisiz oldu.' }

$setupPath = Join-Path $PSScriptRoot "dist\CostAccountingAutomation-Setup-$Version.exe"
if (-not (Test-Path -LiteralPath $setupPath)) {
    throw "Setup dosyasi bulunamadi: $setupPath"
}

# --- 3) Veritabanina yayinlanir ------------------------------------------
Write-Host ''
Write-Host "Adim 2/2 - Guncelleme veritabanina yayinlaniyor ($Version)..." -ForegroundColor Cyan
if (-not $Notes) {
    $Notes = "Surum $Version"
}

$pubArgs = @('-Version', $Version, '-SetupPath', $setupPath, '-Notes', $Notes)
if ($Mandatory) {
    $pubArgs += '-Mandatory'
}

& (Join-Path $PSScriptRoot 'publish-update.ps1') @pubArgs
if (-not $?) { throw 'publish-update basarisiz oldu.' }

Write-Host ''
Write-Host "YAYIN TAMAM: surum $Version" -ForegroundColor Green
Write-Host "Setup dosyasi: $setupPath" -ForegroundColor Green
Write-Host ''
Write-Host 'Kurulu istemciler acilista bu surumu gorup indirecek.' -ForegroundColor Cyan
Write-Host 'Dilersen setup dosyasini ayrica GitHub Releases sayfaniza da yukleyebilirsiniz.' -ForegroundColor Cyan