<#
.SYNOPSIS
    Cost Accounting Automation - EF Core migration komutlari.

.DESCRIPTION
    Projede iki DbContext vardir ve bu yuzden her migration komutunda
    '--context' vermek zorunludur:

      MasterDbContext       -> CostAccountingAutomationMaster veritabani
                               (Companies, CompanyYears, Users, Roles,
                                LoginTokens, Permission)

      ApplicationDbContext  -> Sirket + mali yil is veritabani
                               (CAA_<Yil><_Sirket>)

    Master migration'lari  Migrations\Master klasorune,
    yil migration'lari     Migrations\Year   klasorune uretilir.

    ONEMLI: yil veritabanlarinin adi calisma aninda kullanici secimine gore
    belirlendigi icin yil veritabanlari bu betikle degil, uygulama acilirken
    IAccountingYearProvisioner tarafindan acilip migrate edilir. Bu yuzden
    betikte bilerek 'database update --context ApplicationDbContext' yoktur;
    yanlislikla master'a is verisi yazilmasini engeller.

.DESCRIPTION (ikinci paragraf)
    MERKEZI SUNUCU KURULUMU:

    Birden fazla istemci ayni veritabanina baglanacaksa semayi yalnizca
    yonetici hazirlar. Bu yuzden -Action Provision komutu vardir:

        .\db-migrations.ps1 -Action Provision

    Bu komut master veritabanini olusturur, migration'lari uygular, rol ve
    kullanici tohumlar, sirketler icin icinde bulunulan mali yilin is
    veritabanlarini acar. Ayni anda birden fazla yonetici calistirirsa
    sp_getapplock kilidi sayesinde yalnizca biri calisir; digeri bekler ve
    "yapilacak is yok" diye cikar.

    Uygulamanin kendisi (WinForms) semayi HIC degistirmez; yalnizca salt
    okunur olarak bekleyen migration var mi diye bakar. Bu ayar
    appsettings.json icinde "DatabaseProvisioning": { "Mode": "VerifyOnly" }
    ile yapilir.

    -Action Status ise hicbir sey yazmadan sunucudaki semanin guncel olup
    olmadigini raporlar; destek taleplerinde ilk calistirilacak komuttur.

.EXAMPLE
    .\db-migrations.ps1 -Action Add -Name Mig-1
    .\db-migrations.ps1 -Action UpdateMaster
    .\db-migrations.ps1 -Action List
    .\db-migrations.ps1 -Action Remove
    .\db-migrations.ps1 -Action Script -OutputPath .\scripts\all.sql
    .\db-migrations.ps1 -Action Status
    .\db-migrations.ps1 -Action Provision
#>
[CmdletBinding()]
param(
    [ValidateSet('Add', 'UpdateMaster', 'Remove', 'List', 'Script', 'Status', 'Provision')]
    [string]$Action = 'List',

    # -Action Add icin migration adi (orn. "Mig-1")
    [string]$Name,

    # -Action Script icin cikti dosyasi
    [string]$OutputPath,

    # Veritabani hazirligini sunucuya uygularken kullanilacak ayar dosyasi.
    # Varsayilan: WinForms uygulamasinin appsettings.json dosyasi.
    [string]$SettingsPath
)

$ErrorActionPreference = 'Stop'

$project = 'Cost.Accounting.Automation.Infrastructure'
$common = @('--project', $project, '--startup-project', $project)

# Yil baglamti migration uretimi icin gereken izin. Yalnizca bu betik verir.
$env:CAA_ALLOW_YEAR_DESIGN_TIME = '1'

function Invoke-Ef {
    param([Parameter(Mandatory = $true)][string[]]$EfArgs)

    Write-Host "dotnet ef $($EfArgs -join ' ')" -ForegroundColor DarkGray
    & dotnet ef @EfArgs
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet ef komutu basarisiz oldu ($LASTEXITCODE)."
    }
}

# Veritabani hazirligi (provision) icin ayar dosyasi cozumlemesi.
# Varsayilan, uygulamanin kendi appsettings.json dosyasidir; boylece yonetici
# komutu ile uygulamanin gördugu baglanti dizeleri ayni kaynaktan gelir ve
# ayri bir ayar dosyasi bakim yukunu ortadan kaldirir.
function Resolve-SettingsPath {
    if ($SettingsPath) {
        if (-not (Test-Path -LiteralPath $SettingsPath)) {
            throw "Ayarlar dosyasi bulunamadi: $SettingsPath"
        }

        return (Resolve-Path -LiteralPath $SettingsPath).Path
    }

    $default = Join-Path $PSScriptRoot 'Cost.Accounting.Automation.WinFormsApp\appsettings.json'

    if (Test-Path -LiteralPath $default) {
        return (Resolve-Path -LiteralPath $default).Path
    }

    throw "Varsayilan ayarlar dosyasi bulunamadi: $default (ya da -SettingsPath verin)"
}

function Invoke-Provisioner {
    param([Parameter(Mandatory = $true)][string[]]$CommandArgs)

    $project = 'Cost.Accounting.Automation.Provisioning'
    $settings = Resolve-SettingsPath

    Write-Host "dotnet run --project $project -- $($CommandArgs -join ' ') --settings `"$settings`"" -ForegroundColor DarkGray

    & dotnet run --project $project -- @CommandArgs --settings $settings
    if ($LASTEXITCODE -ne 0) {
        throw "Veritabani hazirligi basarisiz oldu ($LASTEXITCODE)."
    }
}

Push-Location $PSScriptRoot
try {
    switch ($Action) {
        'Add' {
            if ([string]::IsNullOrWhiteSpace($Name)) {
                throw '-Action Add icin -Name parametresi zorunludur. Ornek: -Name Mig-1'
            }

            # 1) Master: yeni context'i tanimlayan migration
            Invoke-Ef -EfArgs (@(
                'migrations', 'add', $Name,
                '--context', 'MasterDbContext',
                '--output-dir', 'Migrations\Master'
            ) + $common)

            # 2) Yil: ayni isimde, ayri klasorde
            Invoke-Ef -EfArgs (@(
                'migrations', 'add', $Name,
                '--context', 'ApplicationDbContext',
                '--output-dir', 'Migrations\Year'
            ) + $common)

            Write-Host ''
            Write-Host 'Iki migration uretildi. Degisiklikleri inceleyip uygulayin:' -ForegroundColor Green
            Write-Host '  .\db-migrations.ps1 -Action UpdateMaster'
        }

        'UpdateMaster' {
            Invoke-Ef -EfArgs (@('database', 'update', '--context', 'MasterDbContext') + $common)
        }

        'Remove' {
            Invoke-Ef -EfArgs (@('migrations', 'remove', '--context', 'MasterDbContext') + $common)
            Invoke-Ef -EfArgs (@('migrations', 'remove', '--context', 'ApplicationDbContext') + $common)
        }

        'List' {
            Write-Host 'Master migration gecmisi:' -ForegroundColor Cyan
            Invoke-Ef -EfArgs (@('migrations', 'list', '--context', 'MasterDbContext') + $common)

            Write-Host ''
            Write-Host 'Yil migration gecmisi:' -ForegroundColor Cyan
            Invoke-Ef -EfArgs (@('migrations', 'list', '--context', 'ApplicationDbContext') + $common)
        }

        'Script' {
            if ([string]::IsNullOrWhiteSpace($OutputPath)) {
                throw '-Action Script icin -OutputPath zorunludur.'
            }

            Invoke-Ef -EfArgs (@(
                'migrations', 'script',
                '--context', 'MasterDbContext',
                '--idempotent',
                '--output', $OutputPath
            ) + $common)
        }

        'Status' {
            # Salt okunur. Sunucudaki sema guncel degilse bekleyen migration
            # kimlikleri listelenir ve yonetim komutu onerilir.
            Invoke-Provisioner -CommandArgs @('status')
        }

        'Provision' {
            # Sema degistiren TEK komut. Yalnizca sunucu yoneticisi calistirir.
            # Uygulama bu isi yapmaz; bkz. DatabaseProvisioning:Mode.
            Invoke-Provisioner -CommandArgs @('provision')
        }
    }
}
finally {
    Pop-Location
}
