using Cost.Accounting.Automation.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cost.Accounting.Automation.Infrastructure.Context;

/// <summary>
/// <c>dotnet ef migrations ...</c> komutları yıl veritabanı için bu fabrikayı
/// kullanır. Migration'lar <c>Migrations\Year</c> klasöründe üretilir ve
/// <c>db-migrations.ps1</c> betiğiyle oluşturulur.
///
/// Yıl veritabanlarının adı çalışma anında ve kullanıcı seçimine bağlıdır;
/// bu yüzden tasarım zamanında hiçbir gerçek katalog kullanılmaz. Bağlantı
/// dizesi bilerek var olmayan bir yer tutucu gösterir ve factory, yalnızca
/// <c>db-migrations.ps1</c> betiğinin ayarladığı
/// <c>CAA_ALLOW_YEAR_DESIGN_TIME</c> ortam değişkeni yoksa hata verir.
/// Böylece <c>database update</c> komutu yanlışlıkla master gibi gerçek bir
/// veritabanına iş verisi yazamaz.
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    internal const string DesignTimeEnvironmentVariable = "CAA_ALLOW_YEAR_DESIGN_TIME";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        if (Environment.GetEnvironmentVariable(DesignTimeEnvironmentVariable) != "1")
        {
            throw new InvalidOperationException(
                "Yıl veritabanı migration'ları yalnızca src\\db-migrations.ps1 betiğiyle üretilir " +
                $"(ya da {DesignTimeEnvironmentVariable}=1 ortam değişkeni elle verilir). " +
                "Yıl veritabanları çalışma anında IAccountingYearProvisioner tarafından açılır; " +
                "master'a veya başka bir kataloga iş verisi yazılmamalıdır.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(DesignTimeConfiguration.GetYearDesignTimeConnectionString());

        return new ApplicationDbContext(optionsBuilder.Options, new DesignTimeClaimContext());
    }
}
