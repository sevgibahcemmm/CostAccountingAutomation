namespace Cost.Accounting.Automation.Application.Devirs;

/// <summary>
/// Devirde hedef yıl veritabanına aktarılacak veri grupları. Kullanıcı form
/// üzerindeki onay kutularından seçim yapar.
/// </summary>
public sealed record DevirOptions
{
    /// <summary>Hesap planı satırları (kod, ad, seviye, üst hesap).</summary>
    public bool ChartOfAccounts { get; init; } = true;

    /// <summary>Birim cinsi ve KDV oranları.</summary>
    public bool TaxRatesAndUnits { get; init; } = true;

    /// <summary>Müşteri/tedarikçi kartları ve açılış bakiyeleri.</summary>
    public bool CurrentAccounts { get; init; } = true;

    /// <summary>Ürün kartları ve stok devir miktarı/değeri.</summary>
    public bool Products { get; init; } = true;

    /// <summary>
    /// Ürün fiyatları. Yeni yılda FIFO/LIFO maliyetleme için giriş katmanı
    /// oluşturduğundan atlanırsa ilk çıkışlarda maliyet 0'a düşer.
    /// </summary>
    public bool ProductPrices { get; init; } = true;

    /// <summary>Reçeteler ve reçete satırları.</summary>
    public bool Recipes { get; init; } = true;

    /// <summary>
    /// Hesap planı yevmiyesine açılış bakiyesi satırları. Mizan raporunun
    /// yeni yılda boş görünmemesi için gereklidir.
    /// </summary>
    public bool ChartBalances { get; init; } = true;

    public bool HasAnything =>
        ChartOfAccounts || TaxRatesAndUnits || CurrentAccounts || Products
        || ProductPrices || Recipes || ChartBalances;
}

public sealed record DevirPreviewResult
{
    public required int TargetYear { get; init; }
    public required string TargetDatabaseName { get; init; }

    /// <summary>Kaynak mali yıl bulunamadıysa false.</summary>
    public required bool HasSource { get; init; }
    public int? SourceYear { get; init; }
    public string? SourceDatabaseName { get; init; }

    /// <summary>Devir neden çalıştırılamayacak, kullanıcıya gösterilecek açıklama.</summary>
    public string? BlockingReason { get; init; }

    /// <summary>
    /// Hedef yıla daha önce devir yapıldıysa o kaydın özeti. Kaynak yılda hiç
    /// bakiye yoksa devir hareket satırı yazmadığı için "hedef yıl boş" kuralı
    /// tekrar devri engellemez; bu kayıt o durumu kapatır.
    /// </summary>
    public DevirLogInfo? PreviousDevir { get; init; }

    public bool CanTransfer => HasSource && BlockingReason is null && PreviousDevir is null;

    public int SourceChartOfAccountCount { get; init; }
    public int TargetChartOfAccountCount { get; init; }
    public int SourceCustomerCount { get; init; }
    public int SourceSupplierCount { get; init; }
    public int SourceProductCount { get; init; }
    public int SourcePriceCount { get; init; }
    public int SourceRecipeCount { get; init; }

    /// <summary>Net bakiyesi sıfırdan farklı cari hesap sayısı.</summary>
    public int SourceCurrentAccountBalanceCount { get; init; }
    public decimal SourceCurrentAccountDebit { get; init; }
    public decimal SourceCurrentAccountCredit { get; init; }

    /// <summary>Net miktarı sıfırdan farklı ürün sayısı.</summary>
    public int SourceStockBalanceCount { get; init; }
    public decimal SourceStockQuantity { get; init; }
    public decimal SourceStockValue { get; init; }

    /// <summary>Bakiyesi sıfırdan farklı hesap sayısı.</summary>
    public int SourceChartBalanceCount { get; init; }
    public decimal SourceChartDebit { get; init; }
    public decimal SourceChartCredit { get; init; }
}

public sealed record DevirLogInfo
{
    public required int SourceYear { get; init; }
    public required int TargetYear { get; init; }
    public required string SourceDatabaseName { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public string? CreatedByName { get; init; }
    public int TotalAdded { get; init; }
}

public sealed record DevirTransferResult
{
    public required int SourceYear { get; init; }
    public required string SourceDatabaseName { get; init; }
    public required string TargetDatabaseName { get; init; }

    public int UnitTypesAdded { get; init; }
    public int TaxRatesAdded { get; init; }
    public int ChartOfAccountsAdded { get; init; }
    public int ChartOfAccountsSkipped { get; init; }
    public int CustomersAdded { get; init; }
    public int SuppliersAdded { get; init; }
    public int ProductsAdded { get; init; }
    public int ProductsSkipped { get; init; }
    public int ProductPricesAdded { get; init; }

    public int ProductPhotosAdded { get; init; }
    public int RecipesAdded { get; init; }
    public int RecipeItemsAdded { get; init; }
    public int CurrentAccountBalancesAdded { get; init; }
    public int StockBalancesAdded { get; init; }
    public int ChartBalancesAdded { get; init; }

    public int TotalAdded =>
        UnitTypesAdded + TaxRatesAdded + ChartOfAccountsAdded + CustomersAdded + SuppliersAdded
        + ProductsAdded + ProductPricesAdded + ProductPhotosAdded + RecipesAdded + RecipeItemsAdded
        + CurrentAccountBalancesAdded + StockBalancesAdded + ChartBalancesAdded;
}
