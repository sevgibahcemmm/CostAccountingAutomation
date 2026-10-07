<#
.SYNOPSIS
    Kurulum paketi icin WinForms istemcisini ve caa-provision aracini hazirlar.

.DESCRIPTION
    Iki uygulamayi Release modunda publish edip 'staging' klasorune koyar:

        staging\app        WinForms istemcisi (Cost.Accounting.Automation.WinFormsApp.exe)
        staging\provision  Veritabani hazirlik araci (caa-provision.exe)

    Paket self-contained uretilir; .NET calisma zamanlari (NETCore, WindowsDesktop,
    AspNetCore) uygulamalarin icine gomulur. Hedef makinede .NET kurulu olmasina
    gerek kalmaz. Tek dis bagimlilik olan VC++ 2015-2022 Redistributable, setup'in
    yanina indirilip gerekirse sessizce kurulur.

    ONEMLI: Gizli deger tasimamak icin publish sonrasi appsettings.Local.json
    dosyalari staging'den SILINIR. Bu dosyayi kurulum sihirbazi olusturur.
    Aksi hâlde gelistirici makinesinin sunucu sifreleri setup.exe icine girer.

    NOT: Bu dosya bilerek yalnizca ASCII karakter icerir. Windows PowerShell 5.1,
    BOM'suz UTF-8 dosyalari ANSI kodlamasiyla okur; Turkce karakterler iceren
    yorumlar satirlari bozup ayristirma hatasi uretir.

.PARAMETER Configuration
    Derleme yapilandirmasi. Varsayilan: Release.

.PARAMETER Runtime
    Hedef calisma zamani. Varsayilan: win-x64.

.EXAMPLE
    .\publish.ps1
#>
[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$Runtime = 'win-x64'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$srcRoot = Join-Path $repoRoot 'src'
$staging = Join-Path $PSScriptRoot 'staging'
$appProject = Join-Path $srcRoot 'Cost.Accounting.Automation.WinFormsApp\Cost.Accounting.Automation.WinFormsApp.csproj'
$provProject = Join-Path $srcRoot 'Cost.Accounting.Automation.Provisioning\Cost.Accounting.Automation.Provisioning.csproj'

Write-Host "Gecici klasor temizleniyor: $staging" -ForegroundColor Cyan
if (Test-Path -LiteralPath $staging) {
    Remove-Item -LiteralPath $staging -Recurse -Force
}

$appOut = Join-Path $staging 'app'
$provOut = Join-Path $staging 'provision'
$runtimeDir = Join-Path $staging 'runtime'
$cacheDir = Join-Path $PSScriptRoot '.cache'
$vcRedistUrl = 'https://aka.ms/vs/17/release/vc_redist.x64.exe'
$localDbUrl = 'https://download.microsoft.com/download/3/8/d/38de7036-2433-4207-8eae-06e247e17b25/SqlLocalDB.msi'

Write-Host 'WinForms istemcisi publish ediliyor (self-contained)...' -ForegroundColor Cyan
dotnet publish $appProject -c $Configuration -r $Runtime --self-contained true -o $appOut
if (-not $?) { throw 'WinForms publish basarisiz oldu.' }

Write-Host 'caa-provision publish ediliyor (self-contained)...' -ForegroundColor Cyan
dotnet publish $provProject -c $Configuration -r $Runtime --self-contained true -o $provOut
if (-not $?) { throw 'Provisioning publish basarisiz oldu.' }

# Gizli deger tasimamak icin yerel ayar ve log dosyalarini staging'den cikar.
foreach ($target in @(
        (Join-Path $appOut 'appsettings.Local.json'),
        (Join-Path $provOut 'appsettings.Local.json'),
        (Join-Path $appOut 'logs'))) {
    if (Test-Path -LiteralPath $target) {
        Remove-Item -LiteralPath $target -Recurse -Force
    }
}

Write-Host ''
Write-Host 'VC++ Redistributable hazirlaniyor...' -ForegroundColor Cyan
New-Item -ItemType Directory -Path $runtimeDir -Force | Out-Null
New-Item -ItemType Directory -Path $cacheDir -Force | Out-Null
$vcRedistCached = Join-Path $cacheDir 'vc_redist.x64.exe'
if (-not (Test-Path -LiteralPath $vcRedistCached)) {
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    Write-Host "Indiriliyor: $vcRedistUrl"
    Invoke-WebRequest -Uri $vcRedistUrl -OutFile $vcRedistCached -UseBasicParsing
    if (-not $?) { throw 'VC++ Redistributable indirilemedi.' }
}
Copy-Item -LiteralPath $vcRedistCached -Destination (Join-Path $runtimeDir 'vc_redist.x64.exe') -Force

Write-Host 'SQL Server 2022 Express LocalDB hazirlaniyor...' -ForegroundColor Cyan
$localDbCached = Join-Path $cacheDir 'SqlLocalDB.msi'
if (-not (Test-Path -LiteralPath $localDbCached)) {
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    Write-Host "Indiriliyor: $localDbUrl"
    Invoke-WebRequest -Uri $localDbUrl -OutFile $localDbCached -UseBasicParsing
    if (-not $?) { throw 'SqlLocalDB.msi indirilemedi.' }
}
Copy-Item -LiteralPath $localDbCached -Destination (Join-Path $runtimeDir 'SqlLocalDB.msi') -Force

Write-Host ''
Write-Host 'Staging hazir:' -ForegroundColor Green
Write-Host "  $appOut"
Write-Host "  $provOut"
Write-Host ''
Write-Host 'Kurulum paketini olusturmak icin: .\build-installer.ps1' -ForegroundColor Cyan