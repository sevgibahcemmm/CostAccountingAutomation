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

.EXAMPLE
    .\db-migrations.ps1 -Action Add -Name Mig-1
    .\db-migrations.ps1 -Action UpdateMaster
    .\db-migrations.ps1 -Action List
    .\db-migrations.ps1 -Action Remove
    .\db-migrations.ps1 -Action Script -OutputPath .\scripts\all.sql
#>
[CmdletBinding()]
param(
    [ValidateSet('Add', 'UpdateMaster', 'Remove', 'List', 'Script')]
    [string]$Action = 'List',

    # -Action Add icin migration adi (orn. "Mig-1")
    [string]$Name,

    # -Action Script icin cikti dosyasi
    [string]$OutputPath
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
    }
}
finally {
    Pop-Location
}
