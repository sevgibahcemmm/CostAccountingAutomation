using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Devirs;

// Bir mali yıl veritabanına devir yapıldığını kalıcı olarak işaretler.
//
// Kayıt, devir transaction'ının İÇİNDE yazılır: böylece devir başarısız olursa
// işaret de geri alınır ve "bu yıl devredildi" bilgisi yalnızca gerçekten
// tamamlanmış devirleri gösterir.
//
// "Hedef yıl boş mu" kontrolü yalnızca fatura/hareket/yevmiye sayılarına bakar.
// Kaynak yılda bakiye yoksa devir hiçbir hareket satırı yazmaz ve hedef yıl
// boş görünmeye devam eder; bu tablodaki kayıt olmadan ikinci bir devir
// çalıştırılabilir ve kullanıcı "devir yapıldı mı?" sorusunu cevaplayamaz.
public sealed class DevirLog : Entity
{
    private DevirLog()
    {
    }

    public DevirLog(int sourceYear, int targetYear, string sourceDatabaseName, string targetDatabaseName)
    {
        if (sourceYear <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceYear));
        }

        if (targetYear <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetYear));
        }

        SourceYear = sourceYear;
        TargetYear = targetYear;
        SourceDatabaseName = sourceDatabaseName;
        TargetDatabaseName = targetDatabaseName;
    }

    /// <summary>Devrin alındığı mali yıl.</summary>
    public int SourceYear { get; private set; }

    /// <summary>Devrin yapıldığı mali yıl.</summary>
    public int TargetYear { get; private set; }

    public string SourceDatabaseName { get; private set; } = string.Empty;

    public string TargetDatabaseName { get; private set; } = string.Empty;

    // Sonuç sayaçları: devir sonrası ekran ve denetim için saklanır.
    public int ChartOfAccountsAdded { get; private set; }

    public int ChartOfAccountsSkipped { get; private set; }

    public int CustomersAdded { get; private set; }

    public int SuppliersAdded { get; private set; }

    public int ProductsAdded { get; private set; }

    public int ProductPricesAdded { get; private set; }

    public int ProductPhotosAdded { get; private set; }

    public int RecipesAdded { get; private set; }

    public int CurrentAccountBalancesAdded { get; private set; }

    public int StockBalancesAdded { get; private set; }

    public int ChartBalancesAdded { get; private set; }

    public int TotalAdded { get; private set; }

    public void SetCounts(
        int chartOfAccountsAdded,
        int chartOfAccountsSkipped,
        int customersAdded,
        int suppliersAdded,
        int productsAdded,
        int productPricesAdded,
        int productPhotosAdded,
        int recipesAdded,
        int currentAccountBalancesAdded,
        int stockBalancesAdded,
        int chartBalancesAdded)
    {
        ChartOfAccountsAdded = chartOfAccountsAdded;
        ChartOfAccountsSkipped = chartOfAccountsSkipped;
        CustomersAdded = customersAdded;
        SuppliersAdded = suppliersAdded;
        ProductsAdded = productsAdded;
        ProductPricesAdded = productPricesAdded;
        ProductPhotosAdded = productPhotosAdded;
        RecipesAdded = recipesAdded;
        CurrentAccountBalancesAdded = currentAccountBalancesAdded;
        StockBalancesAdded = stockBalancesAdded;
        ChartBalancesAdded = chartBalancesAdded;

        TotalAdded = chartOfAccountsAdded + customersAdded + suppliersAdded
            + productsAdded + productPricesAdded + productPhotosAdded + recipesAdded
            + currentAccountBalancesAdded + stockBalancesAdded + chartBalancesAdded;
    }
}
