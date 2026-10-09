<#
.SYNOPSIS
    Sunucudaki guncelleme paylasim klasorunu GitHub'dan cekilen surumle senkronize eder.

.DESCRIPTION
    CI (GitHub Actions) bir surum yayinladiginda (setup.exe + tag) bu betik,
    en son 'latest' release'ini alir ve paylasim klasorune indirir:

        <LocalDir>\CostAccountingAutomation-Setup-1.0.1.exe
        <LocalDir>\update.json

    Sonra Master veritabanindaki AppReleases tablosuna surum kaydini yazar
    (idempotent MERGE). Istemciler acilista bu tablodan en guncel surumu okur;
    yeni surum varsa setup.exe'yi paylasimdan (UNC) kopyalayip kurulumu baslatir.

    NEDEN PULL MODELI?
    GitHub Actions bulut runner'larinda calisir; yerel ag paylasimlarina
    (LAN) ulasamaz. Bu yuzden "CI sunucuya yuklesin" mimarisi teknik olarak
    mumkun degildir. Bunun yerine sunucu (bu betik) GitHub'dan ceker.

    KURULUM (bir kez, sunucu admin'i olarak):
      1. Klasoru paylasin:  icacls / net share
         net share CostAccountingUpdates=D:\CostAccountingUpdates /grant:,,Everyone=READ
      2. Betigi klasore kopyalayin:  D:\CostAccountingUpdates\sync-updates.ps1
      3. Zamanlanmis gorev olusturun (her saat):
         schtasks /Create /SC HOURLY /TN "CostAccountingUpdateSync" /RL HIGHEST ^
           /TR "powershell -NoProfile -ExecutionPolicy Bypass -File D:\CostAccountingUpdates\sync-updates.ps1"
      4. Master veritabani AppReleases tablosu hazir olmali: sunucuda bir kez
         'caa-provision provision' (ya da en azindan migration) calistirin.

.PARAMETER Repo
    GitHub deposu. Varsayilan: sevgibahcemmm/CostAccountingAutomation (genel).

.PARAMETER LocalDir
    Paylasim klasorunun YEREL yolu (sunucudaki dosya sistemi).

.PARAMETER ShareUnc
    Istemcilerin kullandigi paylasim adresi. AppReleases kaydindaki SetupPath
    bu adresle sifirlanir. ORNEK:
        \\192.168.1.5\CostAccountingUpdates

.PARAMETER MasterConnection
    Master veritabani baglanti dizesi. Varsayilan, sunucudaki yerel SQL Express:
        Data Source=.\SQLEXPRESS;Initial Catalog=CostAccountingAutomationMaster;
        Integrated Security=True;Connect Timeout=10;Encrypt=False
    (Windows PowerShell 5.1 'Trust Server Certificate' anahtarini desteklemez.)

.PARAMETER GitHubToken
    Ozel depo icin gerekir; genel depoya gerek yok.

.NOTES
    Bu dosya bilerek yalnizca ASCII karakter icerir (Windows PowerShell 5.1
    uyumlulugu). Turkce karakter kullanmayin.
.EXAMPLE
    .\sync-updates.ps1 -LocalDir D:\CostAccountingUpdates -ShareUnc \\192.168.1.5\CostAccountingUpdates
#>
[CmdletBinding()]
param(
    [string]$Repo = 'sevgibahcemmm/CostAccountingAutomation',
    [string]$LocalDir = 'D:\CostAccountingUpdates',
    [string]$ShareUnc = '',
    [string]$MasterConnection = 'Data Source=.\SQLEXPRESS;Initial Catalog=CostAccountingAutomationMaster;Integrated Security=True;Connect Timeout=10;Encrypt=False',
    [string]$GitHubToken = ''
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ShareUnc)) {
    throw 'ShareUnc parametresi zorunludur. Ornek: -ShareUnc \\192.168.1.5\CostAccountingUpdates'
}

if (-not (Test-Path -LiteralPath $LocalDir)) {
    New-Item -ItemType Directory -Path $LocalDir -Force | Out-Null
}

$manifestPath = Join-Path $LocalDir 'update.json'

Write-Host "[1/5] Son release sorgulaniyor: $Repo" -ForegroundColor Cyan
$headers = @{ 'User-Agent' = 'sync-updates.ps1' }
if ($GitHubToken) {
    $headers['Authorization'] = "Bearer $GitHubToken"
}
$release = Invoke-RestMethod -Uri "https://api.github.com/repos/$Repo/releases/latest" -Headers $headers
$version = $release.tag_name.TrimStart('v')

$asset = $release.assets | Where-Object { $_.name -like 'CostAccountingAutomation-Setup-*.exe' } | Select-Object -First 1
if (-not $asset) {
    throw "Latest release'te setup.exe asset'i bulunamadi."
}

Write-Host ("En son surum: {0}  setup: {1}" -f $version, $asset.name) -ForegroundColor Green

if ((Test-Path -LiteralPath $manifestPath)) {
    try {
        $existing = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
        if ($existing.version -eq $version) {
            Write-Host 'Paylasim zaten guncel; indirme atlandi.'
            exit 0
        }
    } catch {
        # update.json bozuksa yeniden indir.
    }
}

Write-Host "[2/5] setup indiriliyor: $($asset.name)" -ForegroundColor Cyan
$setupPath = Join-Path $LocalDir $asset.name
Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $setupPath -UseBasicParsing

Write-Host "[3/5] Eski setup dosyalari temizleniyor..." -ForegroundColor Cyan
Get-ChildItem -LiteralPath $LocalDir -Filter 'CostAccountingAutomation-Setup-*.exe' |
    Where-Object { $_.FullName -ne $setupPath } |
    Remove-Item -Force

Write-Host "[4/5] update.json yaziliyor (gecmis kayit)... " -ForegroundColor Cyan
$url = ($ShareUnc.TrimEnd('\') + '\' + $asset.name)
$manifest = [ordered]@{
    version   = $version
    url       = $url
    notes     = "Surum $version"
    mandatory = $false
}
$manifest | ConvertTo-Json | Set-Content -LiteralPath $manifestPath -Encoding UTF8

Write-Host "[5/5] AppReleases tablosuna surum kaydi yaziliyor..." -ForegroundColor Cyan
try {
    [void][System.Reflection.Assembly]::Load('System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089')

    $conn = New-Object System.Data.SqlClient.SqlConnection($MasterConnection)
    $conn.Open()
    try {
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = @'
MERGE dbo.AppReleases AS target
USING (VALUES (1)) AS source(Dummy) ON target.[Version] = @version
WHEN MATCHED THEN
    UPDATE SET
        SetupPath   = @setupPath,
        Notes       = @notes,
        Mandatory   = @mandatory,
        PublishedAt = SYSUTCDATETIME(),
        IsActive    = 1
WHEN NOT MATCHED THEN
    INSERT (Id, [Version], SetupPath, Notes, Mandatory, PublishedAt, IsActive)
    VALUES (NEWID(), @version, @setupPath, @notes, @mandatory, SYSUTCDATETIME(), 1);
'@
        $null = $cmd.Parameters.AddWithValue('@version', $version)
        $null = $cmd.Parameters.AddWithValue('@setupPath', $url)
        $null = $cmd.Parameters.AddWithValue('@notes', "Surum $version")
        $null = $cmd.Parameters.AddWithValue('@mandatory', $false)
        $cmd.CommandTimeout = 15
        $null = $cmd.ExecuteNonQuery()
    } finally {
        $conn.Close()
    }
} catch {
    throw "AppReleases kaydi yazilamadi: $($_.Exception.Message)  (Master veritabani hazir degilse sunucuda 'caa-provision provision' calistirin.)"
}

Write-Host ''
Write-Host 'Paylasim guncel:' -ForegroundColor Green
Write-Host ("  AppReleases versiyonu : {0}" -f $version)
Write-Host ("  Setup                 : {0}" -f $url)
Write-Host ''
Write-Host 'NOT: update.json yalnizca kayit icin tutulur; istemciler artik'
Write-Host 'AppReleases tablosundan (Master veritabani) surumu okur.' -ForegroundColor DarkGray