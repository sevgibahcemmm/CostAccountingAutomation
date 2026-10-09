<#
.SYNOPSIS
    Derlenmis setup.exe dosyasini merkezi veritabanina guncelleme olarak yayinlar.

.DESCRIPTION
    'caa-provision publish-update' komutunu calistirarak setup.exe dosyasini
    AppReleases tablosuna yazar. Uygulama acilista bu dosyayi veritabanindan
    indirip kendini gunceller; harici bir sunucu/URL gerekmez.

    Eski surumlerin kurulum dosyalari otomatik temizlenir; veritabaninda yalnizca
    en guncel setup saklanir.

.PARAMETER Version
    Yayinlanacak surum (4 haneli; build-installer.ps1'in urettigi).

.PARAMETER SetupPath
    Yayinlanacak setup.exe tam yolu.

.PARAMETER Notes
    Kullanicilara gosterilecek surum notlari.

.PARAMETER Mandatory
    Zorunlu guncelleme: kullanici "daha sonra/atla" secemez.

.EXAMPLE
    .\publish-update.ps1 -Version 1.0.0.2 -SetupPath ".\dist\CostAccountingAutomation-Setup-1.0.0.2.exe"
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [string]$SetupPath,

    [string]$Notes = "Surum $Version",

    [switch]$Mandatory
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$srcRoot = Join-Path $repoRoot 'src'
$provProject = Join-Path $srcRoot 'Cost.Accounting.Automation.Provisioning\Cost.Accounting.Automation.Provisioning.csproj'

if (-not (Test-Path -LiteralPath $SetupPath)) {
    throw "Setup dosyasi bulunamadi: $SetupPath"
}

# Ayarlariyla birlikte calisacagindan caa-provision gecici klasore yayinlanir.
$tmpProv = Join-Path $env:TEMP 'caa-provision-publishupdate'
if (Test-Path -LiteralPath $tmpProv) {
    Remove-Item -LiteralPath $tmpProv -Recurse -Force
}

Write-Host 'caa-provision yayinlaniyor...' -ForegroundColor Cyan
& dotnet publish $provProject -c Release -r win-x64 --self-contained true -o $tmpProv
if (-not $?) { throw 'caa-provision yayinlanamadi.' }

$provExe = Join-Path $tmpProv 'caa-provision.exe'
$args = @('publish-update', '--file', (Resolve-Path -LiteralPath $SetupPath), '--version', $Version, '--notes', $Notes)
if ($Mandatory) {
    $args += '--mandatory'
}

Write-Host "Yayinlaniyor: surum $Version -> $SetupPath" -ForegroundColor Cyan
& $provExe @args
if ($LASTEXITCODE -ne 0) { throw "publish-update basarisiz oldu (kod $LASTEXITCODE)." }

Write-Host ''
Write-Host "Yayin hazir: $Version" -ForegroundColor Green