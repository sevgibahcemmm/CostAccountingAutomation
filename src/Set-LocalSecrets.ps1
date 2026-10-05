<#
.SYNOPSIS
    Bu makine icin gizli yapilandirma degerlerini uretir ve kaydeder.

.DESCRIPTION
    Uygulama JWT imzalama anahtarini appsettings.json dosyasindan okumaz; anahtar
    iki kanaldan gelebilir:

      1. Ortam degiskeni (Jwt__SecretKey)      - varsayilan
      2. appsettings.Local.json                 - -LocalFile ile

    Oncelik sirasi: appsettings.json < appsettings.Local.json < ortam degiskeni.
    appsettings.Local.json .gitignore'da ignore edilir ve kurulum klasoruyle
    birlikte kopyalanabilir; bu sayede kurulan istemci makinesinde kullanicinin
    setx calistirmasina gerek kalmaz.

    Bu betik 256 bitlik rastgele bir anahtar uretir ve ekrana basmaz.

    Guvenlik notu:
      - appsettings.json ASLA guncellenmemelidir; dosya izlenir ve anahtar tüm
        commit gecmisine yayilir.
      - appsettings.Local.json makinede plaintext olarak durur. Yalnizca makineye
        erisebilen kullanicinin erisebilecegi bir makinede tercih edin.
      - Ortam degiskeni yontemi daha guvenlidir ama setx yalnizca yeni acilan
        oturumlarda gorunur; acik olan Visual Studio'yu yeniden baslatmaniz gerekir.

    Anahtar degistiginde daha once uretilmis token'lar gecersizlesir ve kullanici
    yeniden giris yapmak zorunda kalir. Bu beklenen davranistir.

    NOT: Bu dosya bilerek yalnizca ASCII karakter icerir. Windows PowerShell 5.1,
    BOM'suz UTF-8 dosyalari ANSI kodlamasiyla okur; Turkce karakterler iceren
    yorumlar satirlari bozup ayristirma hatasi uretir.

.PARAMETER Rotate
    Var olan anahtarin yerine yenisi uretir.

.PARAMETER LocalFile
    Anahtari ortam degiskeni yerine appsettings.Local.json dosyasina yaz.
    Her iki uygulamanin (WinFormsApp ve Provisioning) klasorune ayri ayri yazar,
    boylece ikisi de ayni anahtari kullanir.

.EXAMPLE
    .\Set-LocalSecrets.ps1
    .\Set-LocalSecrets.ps1 -Rotate
    .\Set-LocalSecrets.ps1 -LocalFile
#>
[CmdletBinding()]
param(
    # Var olan anahtarin uzerine yaz.
    [switch]$Rotate,

    # Anahtari appsettings.Local.json dosyasina yaz (ortam degiskeni yerine).
    [switch]$LocalFile
)

$ErrorActionPreference = 'Stop'

# HS512 icin gereken en kisa anahtar 64 bayt. 128 karakter (256 bit) uretiyoruz:
# gecerli ve uretimde de kullanilabilecek kadar genis bir pay.
$keyLength = 128

$targets = @(
    (Join-Path $PSScriptRoot 'Cost.Accounting.Automation.WinFormsApp\appsettings.Local.json'),
    (Join-Path $PSScriptRoot 'Cost.Accounting.Automation.Provisioning\appsettings.Local.json')
)

if ($LocalFile) {
    # Oncelik tuzagi: ortam degiskeni dosyanin UZERINE yazar. Ikisi de tanimliysa
    # ortam degiskeni gecerlidir ve dosyadaki deger sessizce yok sayilir. Bu
    # durumda kullanici hangi anahtarin etkin oldugunu bilemez; burada bildirilir.
    $envKey = [Environment]::GetEnvironmentVariable('Jwt__SecretKey', 'User')

    if ($envKey) {
        Write-Host ''
        Write-Host 'UYARI: Jwt__SecretKey ortam degiskeni zaten tanimli.' -ForegroundColor Red
        Write-Host '       Yapilandirma onceligi: appsettings.json < appsettings.Local.json < ortam degiskeni' -ForegroundColor Red
        Write-Host '       Yani dosyaya yazacaginiz anahtar ETKIN OLMAYACAK; ortam degiskeni kazanir.' -ForegroundColor Red
        Write-Host ''
        Write-Host 'Ortam degiskenini silmek icin:' -ForegroundColor Yellow
        Write-Host '  [Environment]::SetEnvironmentVariable(''Jwt__SecretKey'', $null, ''User'')' -ForegroundColor Yellow
        Write-Host ''
    }

    foreach ($target in $targets) {
        if ((Test-Path -LiteralPath $target) -and -not $Rotate) {
            Write-Host "Zaten var: $target" -ForegroundColor Yellow
            Write-Host 'Uzerine yazmak icin: .\Set-LocalSecrets.ps1 -LocalFile -Rotate' -ForegroundColor Yellow
            return
        }
    }
}
else {
    $existing = [Environment]::GetEnvironmentVariable('Jwt__SecretKey', 'User')

    if ($existing -and -not $Rotate) {
        Write-Host 'Jwt__SecretKey zaten tanimli.' -ForegroundColor Yellow
        Write-Host 'Degistirmek icin: .\Set-LocalSecrets.ps1 -Rotate' -ForegroundColor Yellow
        Write-Host ''
        Write-Host "Anahtar ekrana basilmaz; uzunlugu: $($existing.Length) karakter." -ForegroundColor DarkGray
        return
    }

    if ($existing) {
        Write-Host 'Mevcut anahtar degistiriliyor. Daha once uretilmis token''lar gecersizlesir.' -ForegroundColor Yellow
    }
}

# Kriptografik olarak guvenli rastgelelik. [System.Random] bilerek kullanilmaz.
#
# RandomNumberGenerator.Fill(Span<byte>) .NET Core/.NET 5+ icindir; bu betik
# Windows PowerShell 5.1 ile calisir ve .NET Framework 4.x uzerinde yasar.
# Bu yuzden eski ve her ikisinde de var olan Create()/GetBytes() kullanilir.
$bytes = [byte[]]::new($keyLength)
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
try {
    $rng.GetBytes($bytes)
}
finally {
    $rng.Dispose()
}

$key = [Convert]::ToBase64String($bytes)

if ($LocalFile) {
    foreach ($target in $targets) {
        $content = @"
{
  "Jwt": {
    "SecretKey": "$key"
  }
}
"@

        Set-Content -LiteralPath $target -Value $content -Encoding UTF8
        Write-Host "Yazildi: $target" -ForegroundColor Green
    }

    Write-Host ''
    Write-Host 'Anahtar bu dosyalarda makinede plaintext olarak duruyor.' -ForegroundColor Yellow
    Write-Host '.gitignore bu dosyalari izlemiyor; yine de ekran goruntusu, yedek veya' -ForegroundColor Yellow
    Write-Host 'destek paketi ile paylasilabilir. Daha guvenli yontem: -LocalFile kullanmadan' -ForegroundColor Yellow
    Write-Host 'ortam degiskeni ile calistirmak.' -ForegroundColor Yellow
    Write-Host ''
    Write-Host 'Simdi acilan Visual Studio veya terminal penceresini yeniden baslatin.' -ForegroundColor Cyan
}
else {
    [Environment]::SetEnvironmentVariable('Jwt__SecretKey', $key, 'User')

    # Bu oturum icin de gecerli olsun; betigi calistirdiktan sonra programi hemen
    # denemek isteyen gelistirici yeni terminal acmak zorunda kalmasin.
    $env:Jwt__SecretKey = $key

    Write-Host ''
    Write-Host 'Jwt__SecretKey uretildi ve kaydedildi.' -ForegroundColor Green
    Write-Host '  Kayit yeri : Kullanici ortam degiskeni' -ForegroundColor DarkGray
    Write-Host "  Uzunluk    : $($key.Length) karakter (HS512 icin en az 64 gerekir)" -ForegroundColor DarkGray
    Write-Host ''
    Write-Host 'Anahtar ekrana basilmadi ve su dosyaya da yazilmamali:' -ForegroundColor Yellow
    Write-Host '  src\Cost.Accounting.Automation.WinFormsApp\appsettings.json' -ForegroundColor Yellow
    Write-Host ''
    Write-Host 'Ortam degiskeni yerine dosya kullanmak isterseniz:' -ForegroundColor Cyan
    Write-Host '  .\Set-LocalSecrets.ps1 -LocalFile' -ForegroundColor Cyan
    Write-Host ''
    Write-Host 'Simdi acilan Visual Studio veya terminal penceresini yeniden baslatin.' -ForegroundColor Cyan
}
