<#
.SYNOPSIS
    Kurulum paketi icin WinForms istemcisini ve caa-provision aracini hazirlar.

.DESCRIPTION
    Iki uygulamayi Release modunda publish edip 'staging' klasorune koyar:

        staging\app        WinForms istemcisi (Maliyet Muhasebesi Otomasyonu.exe)
        staging\provision  Veritabani hazirlik araci (caa-provision.exe)

    Paket self-contained uretilir; .NET calisma zamanlari (NETCore, WindowsDesktop,
    AspNetCore) uygulamalarin icine gomulur. Hedef makinede .NET kurulu olmasina
    gerek kalmaz. Tek dis bagimlilik olan VC++ 2015-2022 Redistributable, setup'in
    yanina indirilip gerekirse sessizce kurulur.

    ONEMLI: Gizli deger tasimamak icin publish sonrasi appsettings.Local.json
    dosyalari staging'den SILINIR. Bu dosyayi kurulum sihirbazi olusturur.
    Aksi halde gelistirici makinesinin sunucu sifreleri setup.exe icine girer.

    NOT: Bu dosya bilerek yalnizca ASCII karakter icerir. Windows PowerShell 5.1,
    BOM'suz UTF-8 dosyalari ANSI kodlamasiyla okur; Turkce karakterler iceren
    yorumlar satirlari bozup ayristirma hatasi uretir.

.PARAMETER Configuration
    Derleme yapilandirmasi. Varsayilan: Release.

.PARAMETER Runtime
    Hedef calisma zamani. Varsayilan: win-x64.

.PARAMETER Version
    Derleme/assembly surumu (4 haneli, ornek: 1.0.0.2). Verilirse
    'dotnet publish' komutuna -p:Version olarak gecilir ve hem exe'nin
    dosya/assembly surumu hem de guncelleme karsilastirmasi dogru olur.
    Bos birakilirsa assembly surumu csproj'daki <Version> olur (genelde 1.0.0).

.EXAMPLE
    .\publish.ps1 -Version 1.0.0.2
#>
[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$Runtime = 'win-x64',
    [string]$Version = ''
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

$versionArgs = @()
if ($Version) {
    $versionArgs = @("-p:Version=$Version")
    Write-Host "Assembly/derleme surumu: $Version"
}

Write-Host 'WinForms istemcisi publish ediliyor (self-contained)...' -ForegroundColor Cyan
& dotnet publish $appProject -c $Configuration -r $Runtime --self-contained true -o $appOut @versionArgs
if (-not $?) { throw 'WinForms publish basarisiz oldu.' }

# Kullanici istedigi isim: exe Turkce adla kurulur.
$appExeOld = Join-Path $appOut 'Cost.Accounting.Automation.WinFormsApp.exe'
$appExeNew = Join-Path $appOut 'Maliyet Muhasebesi Otomasyonu.exe'
if (Test-Path -LiteralPath $appExeOld) {
    Rename-Item -LiteralPath $appExeOld -NewName (Split-Path -Leaf $appExeNew)
    Write-Host "Exe yeniden adlandirildi: $(Split-Path -Leaf $appExeNew)" -ForegroundColor Green
}

Write-Host 'caa-provision publish ediliyor (self-contained)...' -ForegroundColor Cyan
& dotnet publish $provProject -c $Configuration -r $Runtime --self-contained true -o $provOut @versionArgs
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

Write-Host ''
Write-Host 'Staging hazir:' -ForegroundColor Green
Write-Host "  $appOut"
Write-Host "  $provOut"
Write-Host ''
Write-Host 'Kurulum paketini olusturmak icin: .\build-installer.ps1' -ForegroundColor Cyan