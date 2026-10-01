using Cost.Accounting.Automation.Application.Devirs;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.AccountingYears;
using Cost.Accounting.Automation.Domain.AccountingYears.ValueObjects;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Companies.ValueObjects;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Devirs;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Photos;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Products.TaxRates;
using Cost.Accounting.Automation.Domain.Products.ValueObjects;
using Cost.Accounting.Automation.Domain.Recipes;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.StockIssues;
using Cost.Accounting.Automation.Domain.Suppliers;
using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Infrastructure.Context;
using Cost.Accounting.Automation.Infrastructure.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Cost.Accounting.Automation.Infrastructure.Services;

/// <summary>
/// Aynı şirketin bir önceki mali yılından seçili yıla devir (açılış bakiyesi)
/// yapar.
///
/// Kaynak yıl veritabanına DI ile kayıtlı <see cref="ApplicationDbContext"/>
/// üzerinden değil, adı çalışma anında belirlendiği için doğrudan kurulan
/// seçeneklerle bağlanılır (bkz. <see cref="YearDatabaseProvisioner"/>).
///
/// Kaynak yıldaki kayıtların kimlikleri hedefe kopyalanmaz; her grup için
/// kaynak kimlik → hedef kimlik eşlemesi kurulur. Böylece hedefte zaten var
/// olan kayıtlar yeniden kullanılabilir ve yabancı anahtar (FK) ihlali yaşanmaz.
/// </summary>
internal sealed class DevirTransferService(
    ApplicationDbContext targetContext,
    MasterDbContext masterContext,
    IAccountingDbSelector dbSelector,
    IClaimContext claimContext) : IDevirTransferService
{
    /// <summary>Devirden gelen yevmiye satırlarının kaynak tipi.</summary>
    private const string DevirSourceType = "Devir";

    private const decimal StockTolerance = 0.0001m;
    private const decimal MoneyTolerance = 0.005m;

    public async Task<DevirPreviewResult> BuildPreviewAsync(CancellationToken cancellationToken = default)
    {
        TargetSelection target = RequireTargetSelection();
        int targetAccountCount = await targetContext.Set<ChartOfAccount>().CountAsync(cancellationToken);
        string? blocking = await FindTargetBlockingReasonAsync(cancellationToken);
        DevirLogInfo? previous = await ReadDevirLogAsync(target.Year, cancellationToken);

        SourceYearRef? source = await FindPreviousYearAsync(target, cancellationToken);

        if (source is null)
        {
            return new DevirPreviewResult
            {
                TargetYear = target.Year,
                TargetDatabaseName = RequireTargetSelection().DatabaseName,
                HasSource = false,
                TargetChartOfAccountCount = targetAccountCount,
                PreviousDevir = previous,
                BlockingReason = blocking
            };
        }

        await using ApplicationDbContext sourceContext = OpenYearContext(source.DatabaseName);

        if (!await sourceContext.Database.CanConnectAsync(cancellationToken))
        {
            return new DevirPreviewResult
            {
                TargetYear = target.Year,
                TargetDatabaseName = RequireTargetSelection().DatabaseName,
                HasSource = true,
                SourceYear = source.Year,
                SourceDatabaseName = source.DatabaseName,
                TargetChartOfAccountCount = targetAccountCount,
                PreviousDevir = previous,
                BlockingReason = blocking
                    ?? $"Kaynak yıl veritabanına bağlanılamadı: {source.DatabaseName}"
            };
        }

        List<CurrentAccountAggregate> cari =
            await ReadCurrentAccountAggregateAsync(sourceContext, cancellationToken);
        List<StockAggregate> stock = await ReadStockAggregateAsync(sourceContext, cancellationToken);
        List<ChartAggregate> chart = await ReadChartAggregateAsync(sourceContext, cancellationToken);

        List<CurrentAccountAggregate> cariBalanced =
            [.. cari.Where(r => Math.Abs(r.Debit - r.Credit) > MoneyTolerance)];
        List<StockAggregate> stockBalanced = [.. stock.Where(r => Math.Abs(r.Quantity) > StockTolerance)];
        List<ChartAggregate> chartBalanced =
            [.. chart.Where(r => r.Debit > MoneyTolerance || r.Credit > MoneyTolerance)];

        return new DevirPreviewResult
        {
            TargetYear = target.Year,
            TargetDatabaseName = RequireTargetSelection().DatabaseName,
            HasSource = true,
            SourceYear = source.Year,
            SourceDatabaseName = source.DatabaseName,
            BlockingReason = blocking,
            PreviousDevir = previous,
            SourceChartOfAccountCount = await sourceContext.Set<ChartOfAccount>().CountAsync(cancellationToken),
            TargetChartOfAccountCount = targetAccountCount,
            SourceCustomerCount = await sourceContext.Set<Customer>().CountAsync(cancellationToken),
            SourceSupplierCount = await sourceContext.Set<Supplier>().CountAsync(cancellationToken),
            SourceProductCount = await sourceContext.Set<Product>().CountAsync(cancellationToken),
            SourcePriceCount = await sourceContext.Set<ProductPrice>().CountAsync(cancellationToken),
            SourceRecipeCount = await sourceContext.Set<Recipe>().CountAsync(cancellationToken),
            SourceCurrentAccountBalanceCount = cariBalanced.Count,
            SourceCurrentAccountDebit = cariBalanced.Sum(r => r.Debit),
            SourceCurrentAccountCredit = cariBalanced.Sum(r => r.Credit),
            SourceStockBalanceCount = stockBalanced.Count,
            SourceStockQuantity = stockBalanced.Sum(r => r.Quantity),
            SourceStockValue = stockBalanced.Sum(r => r.Value),
            SourceChartBalanceCount = chartBalanced.Count,
            SourceChartDebit = chartBalanced.Sum(r => r.Debit),
            SourceChartCredit = chartBalanced.Sum(r => r.Credit)
        };
    }

    public async Task<DevirTransferResult> TransferAsync(
        DevirOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        TargetSelection target = RequireTargetSelection();
        SourceYearRef source = await FindPreviousYearAsync(target, cancellationToken)
            ?? throw new InvalidOperationException("Devir alınacak bir önceki mali yıl bulunamadı.");

        await using ApplicationDbContext sourceContext = OpenYearContext(source.DatabaseName);

        DateOnly openingDate = await ResolveOpeningDateAsync(target, cancellationToken);
        string documentNo = BuildDevirDocumentNo(source.Year);

        // Devir hedef yılı kalıcı olarak değiştirir. Ön önizleme ile başlatma
        // arasında hedefe hareket yazılmış olabilir; bu yüzden boşluk şartı
        // yazma anında yeniden doğrulanır.
        string? lateBlocking = await FindTargetBlockingReasonAsync(cancellationToken);
        if (lateBlocking is not null)
        {
            throw new InvalidOperationException(lateBlocking);
        }

        // Kaynak yılda hiç bakiye yoksa devir hiçbir hareket satırı yazmaz ve
        // "hedef yıl boş" kuralı ikinci devri engellemez. DevirLog bu yılın
        // zaten devredildiğini kalıcı olarak bildirir.
        DevirLogInfo? existingLog = await ReadDevirLogAsync(target.Year, cancellationToken);
        if (existingLog is not null)
        {
            throw new InvalidOperationException(
                $"{target.Year} mali yılı zaten {existingLog.SourceYear} mali yılından devredilmiş "
                + $"({existingLog.CreatedAt:d.MM.yyyy HH:mm}). Mükerrer devir yapılamaz.");
        }


        // Hedef yılın yarım kalmaması için tüm yazmalar tek transaction
        // içinde olmalıdır. Aksi hâlde bir hata, hedefte hareket kaydı bırakır
        // ve "hedef yıl boş değil" kuralı devri kalıcı olarak engeller.
        //
        // Bağlantı SqlServerRetryingExecutionStrategy kullanır; bu strateji
        // kullanıcı tarafından açılan transaction'ı desteklemez. Bu yüzden
        // transaction, stratejinin yeniden deneyebileceği birim olarak
        // çalıştırılır ve her denemede değişiklik izleri temizlenir.
        IExecutionStrategy strategy = targetContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync<DevirTransferResult>(
            async ct =>
            {
                targetContext.ChangeTracker.Clear();

                await using var transaction = await targetContext.Database.BeginTransactionAsync(ct);

                try
                {
                    DevirTransferResult result = await TransferCoreAsync(
                        options, source, sourceContext, openingDate, documentNo, ct);

                    await transaction.CommitAsync(ct);
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            },
            cancellationToken);
    }

    private async Task<DevirTransferResult> TransferCoreAsync(
        DevirOptions options,
        SourceYearRef source,
        ApplicationDbContext sourceContext,
        DateOnly openingDate,
        string documentNo,
        CancellationToken cancellationToken)
    {
        var accountMap = new Dictionary<Guid, Guid>();
        var customerMap = new Dictionary<Guid, Guid>();
        var supplierMap = new Dictionary<Guid, Guid>();
        var productMap = new Dictionary<Guid, Guid>();
        var unitMap = new Dictionary<Guid, Guid>();
        var taxRateMap = new Dictionary<Guid, Guid>();

        int units = 0, taxRates = 0, accounts = 0, accountsSkipped = 0;
        int customers = 0, suppliers = 0, products = 0, productsSkipped = 0;
        int prices = 0, photos = 0, recipes = 0, recipeItems = 0;
        int cariBalances = 0, stockBalances = 0, chartBalances = 0;

        // 1) Birim cinsi ve KDV oranları. Yıl veritabanı bunları zaten tohumlar;
        //    eksik olanlar eklenir, hedefteki oranlar korunur.
        if (options.TaxRatesAndUnits)
        {
            units = await CopyUnitTypesAsync(sourceContext, unitMap, cancellationToken);
            taxRates = await CopyTaxRatesAsync(sourceContext, taxRateMap, cancellationToken);
            await targetContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // Seçenek kapalıyken de eşleme kurulur; ürünler hedefteki mevcut
            // birim/KDV kayıtlarına bağlanabilsin diye. Hiçbir şey yazılmaz.
            await MapExistingAsync<ProductUnitType>(sourceContext, unitMap, cancellationToken);
            await MapExistingAsync<TaxRate>(sourceContext, taxRateMap, cancellationToken);
        }

        // 2) Hesap planı: üst hesaplar alt hesaplardan önce eklenir.
        var addedAccountSourceIds = new List<Guid>();
        if (options.ChartOfAccounts)
        {
            (accounts, accountsSkipped, addedAccountSourceIds) =
                await CopyChartOfAccountsAsync(sourceContext, accountMap, cancellationToken);
        }
        else
        {
            accountsSkipped = await MapExistingAsync<ChartOfAccount>(sourceContext, accountMap, cancellationToken);
        }

        await targetContext.SaveChangesAsync(cancellationToken);

        // Üretim bağlantıları YALNIZCA bu adımda eklenen hesaplara yazılır;
        // hedefte önceden var olan hesapların bağlantıları korunur.
        await ResolveProductionLinksAsync(
            sourceContext, accountMap, addedAccountSourceIds, cancellationToken);
        await targetContext.SaveChangesAsync(cancellationToken);

        // 3) Cari kartlar.
        if (options.CurrentAccounts)
        {
            (customers, _) = await CopyCustomersAsync(sourceContext, customerMap, cancellationToken);
            (suppliers, _) = await CopySuppliersAsync(sourceContext, supplierMap, cancellationToken);
        }
        else
        {
            await MapExistingAsync<Customer>(sourceContext, customerMap, cancellationToken);
            await MapExistingAsync<Supplier>(sourceContext, supplierMap, cancellationToken);
        }

        await targetContext.SaveChangesAsync(cancellationToken);

        // 4) Ürün kartları, fiyatları ve görselleri.
        if (options.Products)
        {
            (products, productsSkipped, prices, photos) = await CopyProductsAsync(
                sourceContext, productMap, accountMap, unitMap, taxRateMap,
                options.ProductPrices, cancellationToken);
        }
        else
        {
            productsSkipped = await MapExistingAsync<Product>(sourceContext, productMap, cancellationToken);
        }

        await targetContext.SaveChangesAsync(cancellationToken);

        // 5) Reçeteler (yalnızca eklenen ürünlere ait olanlar).
        if (options.Recipes)
        {
            (recipes, recipeItems) = await CopyRecipesAsync(sourceContext, productMap, cancellationToken);
        }

        await targetContext.SaveChangesAsync(cancellationToken);

        // 6) Devir bakiyeleri: cari, stok ve hesap planı yevmiyesi.
        if (options.CurrentAccounts)
        {
            cariBalances = await WriteCurrentAccountBalancesAsync(
                sourceContext, customerMap, supplierMap, openingDate, documentNo, cancellationToken);
        }

        if (options.Products)
        {
            stockBalances = await WriteStockBalancesAsync(
                sourceContext, productMap, openingDate, documentNo, cancellationToken);
        }

        if (options.ChartBalances)
        {
            chartBalances = await WriteChartBalancesAsync(
                sourceContext, accountMap, openingDate, documentNo, cancellationToken);
        }

        await targetContext.SaveChangesAsync(cancellationToken);

        var result = new DevirTransferResult
        {
            SourceYear = source.Year,
            SourceDatabaseName = source.DatabaseName,
            TargetDatabaseName = RequireTargetSelection().DatabaseName,
            UnitTypesAdded = units,
            TaxRatesAdded = taxRates,
            ChartOfAccountsAdded = accounts,
            ChartOfAccountsSkipped = accountsSkipped,
            CustomersAdded = customers,
            SuppliersAdded = suppliers,
            ProductsAdded = products,
            ProductsSkipped = productsSkipped,
            ProductPricesAdded = prices,
            ProductPhotosAdded = photos,
            RecipesAdded = recipes,
            RecipeItemsAdded = recipes == 0 ? 0 : recipeItems,
            CurrentAccountBalancesAdded = cariBalances,
            StockBalancesAdded = stockBalances,
            ChartBalancesAdded = chartBalances
        };

        // Devir kaydı transaction'ın içinde yazılır: devir başarısız olursa
        // işaret de geri alınır, "bu yıl devredildi" bilgisi yalnızca gerçekten
        // tamamlanmış devirleri gösterir.
        var log = new DevirLog(
            source.Year, RequireTargetSelection().Year, source.DatabaseName,
            RequireTargetSelection().DatabaseName);

        log.SetCounts(
            accounts, accountsSkipped, customers, suppliers, products,
            prices, photos, recipes, cariBalances, stockBalances, chartBalances);

        targetContext.Set<DevirLog>().Add(log);
        await targetContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    /// <summary>
    /// Hedef yıl için daha önce yazılmış devir kaydını okur.
    /// </summary>
    private async Task<DevirLogInfo?> ReadDevirLogAsync(int targetYear, CancellationToken cancellationToken)
    {
        List<DevirLog> logs = await targetContext.Set<DevirLog>()
            .AsNoTrackingWithIdentityResolution()
            .Where(l => l.TargetYear == targetYear)
            .ToListAsync(cancellationToken);

        DevirLog? log = logs
            .OrderByDescending(l => l.CreatedAt)
            .FirstOrDefault();

        if (log is null)
        {
            return null;
        }

        string? createdByName = log.CreatedBy == default
            ? null
            : await masterContext.Set<User>()
                .AsNoTrackingWithIdentityResolution()
                .Where(u => u.Id == log.CreatedBy)
                .Select(u => u.FullName.Value)
                .FirstOrDefaultAsync(cancellationToken);

        return new DevirLogInfo
        {
            SourceYear = log.SourceYear,
            TargetYear = log.TargetYear,
            SourceDatabaseName = log.SourceDatabaseName,
            CreatedAt = log.CreatedAt,
            CreatedByName = createdByName,
            TotalAdded = log.TotalAdded
        };
    }

    // ------------------------------------------------------------------ hedef

    private TargetSelection RequireTargetSelection()
    {
        if (dbSelector.CompanyId is not { } companyId || dbSelector.Year is not { } year)
        {
            throw new InvalidOperationException(
                "Devir işlemi için önce şirket ve mali yıl seçmelisiniz.");
        }

        if (string.IsNullOrWhiteSpace(dbSelector.DatabaseName))
        {
            throw new InvalidOperationException("Seçili mali yılın veritabanı bilgisi yok.");
        }

        return new TargetSelection(companyId, year.Value, dbSelector.DatabaseName);
    }

    /// <summary>
    /// Hedef yılda hareket varsa devir yapılmaz: mükerrer bakiye ve karışık
    /// ekstre riski oluşur.
    /// </summary>
    private async Task<string?> FindTargetBlockingReasonAsync(CancellationToken cancellationToken)
    {
        int invoices = await targetContext.Set<Invoice>().CountAsync(cancellationToken);
        int movements = await targetContext.Set<ProductMovement>().CountAsync(cancellationToken);
        int current = await targetContext.Set<CurrentAccountMovement>().CountAsync(cancellationToken);
        int ledger = await targetContext.Set<ChartOfAccountLedger>().CountAsync(cancellationToken);
        int stockIssues = await targetContext.Set<StockIssue>().CountAsync(cancellationToken);
        int costSlips = await targetContext.Set<CostSlip>().CountAsync(cancellationToken);

        List<string> parts = [];
        if (invoices > 0) parts.Add($"{invoices} fatura");
        if (movements > 0) parts.Add($"{movements} stok hareketi");
        if (current > 0) parts.Add($"{current} cari hareket");
        if (ledger > 0) parts.Add($"{ledger} yevmiye kaydı");
        if (stockIssues > 0) parts.Add($"{stockIssues} stok fişi");
        if (costSlips > 0) parts.Add($"{costSlips} maliyet pusulası");

        return parts.Count == 0
            ? null
            : "Devir yalnızca boş mali yıl veritabanına yapılabilir. Hedef yılda hareket var: "
              + string.Join(", ", parts) + ".";
    }

    private async Task<SourceYearRef?> FindPreviousYearAsync(
        TargetSelection target,
        CancellationToken cancellationToken)
    {
        // Yıl kayıtları çok az sayıdadır; sorgu kalıbı mevcut repository ile aynıdır.
        List<CompanyYear> years = await masterContext.Set<CompanyYear>()
            .AsNoTrackingWithIdentityResolution()
            .Where(cy => cy.CompanyId == target.CompanyId)
            .ToListAsync(cancellationToken);

        return years
            .Where(cy => cy.Year.Value < target.Year)
            .OrderByDescending(cy => cy.Year.Value)
            .Select(cy => new SourceYearRef(cy.Year.Value, cy.DatabaseName.Value))
            .FirstOrDefault();
    }

    private ApplicationDbContext OpenYearContext(string databaseName)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(dbSelector.BuildConnectionString(databaseName));
        optionsBuilder.AddInterceptors(new SqlTimingInterceptor());

        return new ApplicationDbContext(optionsBuilder.Options, claimContext);
    }

    private async Task<DateOnly> ResolveOpeningDateAsync(
        TargetSelection target,
        CancellationToken cancellationToken)
    {
        CompanyYear? record = await masterContext.Set<CompanyYear>()
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(
                cy => cy.DatabaseName == new DatabaseName(target.DatabaseName),
                cancellationToken);

        DateTimeOffset opening = record?.OpeningDate
            ?? new DateTimeOffset(target.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);

        return DateOnly.FromDateTime(opening.LocalDateTime);
    }

    private static string BuildDevirDocumentNo(int sourceYear) => $"DEVIR-{sourceYear}";

    // ------------------------------------------------------------ kaynak okuma

    private static async Task<List<CurrentAccountAggregate>> ReadCurrentAccountAggregateAsync(
        ApplicationDbContext source,
        CancellationToken cancellationToken)
        => await source.Set<CurrentAccountMovement>()
            .AsNoTracking()
            .GroupBy(m => new { Type = m.CurrentAccountType, m.CustomerId, m.SupplierId })
            .Select(g => new CurrentAccountAggregate(
                g.Key.Type,
                g.Key.CustomerId == null ? null : g.Key.CustomerId.Value,
                g.Key.SupplierId == null ? null : g.Key.SupplierId.Value,
                g.Sum(x => x.Debit),
                g.Sum(x => x.Credit)))
            .ToListAsync(cancellationToken);

    private static async Task<List<StockAggregate>> ReadStockAggregateAsync(
        ApplicationDbContext source,
        CancellationToken cancellationToken)
        => await source.Set<ProductMovement>()
            .AsNoTracking()
            .GroupBy(m => m.ProductId)
            .Select(g => new StockAggregate(
                g.Key.Value,
                g.Sum(x => x.MovementType == ProductMovementType.Input ? x.Quantity : -x.Quantity),
                g.Sum(x => (x.MovementType == ProductMovementType.Input ? x.Quantity : -x.Quantity)
                          * (x.UnitPrice == null ? 0m : x.UnitPrice.Value))))
            .ToListAsync(cancellationToken);

    private static async Task<List<ChartAggregate>> ReadChartAggregateAsync(
        ApplicationDbContext source,
        CancellationToken cancellationToken)
        => await source.Set<ChartOfAccountLedger>()
            .AsNoTracking()
            .GroupBy(l => l.ChartOfAccountId)
            .Select(g => new ChartAggregate(
                g.Key.Value,
                g.Sum(x => x.DebitAmount),
                g.Sum(x => x.CreditAmount)))
            .ToListAsync(cancellationToken);

    // ---------------------------------------------------------------- kopyalar

    private async Task<int> CopyUnitTypesAsync(
        ApplicationDbContext source,
        Dictionary<Guid, Guid> map,
        CancellationToken cancellationToken)
    {
        List<NamedRow> sourceRows = await source.Set<ProductUnitType>()
            .AsNoTracking()
            .Select(u => new NamedRow(u.Id, u.Name.Value))
            .ToListAsync(cancellationToken);

        List<ProductUnitType> targetRows = await targetContext.Set<ProductUnitType>()
            .ToListAsync(cancellationToken);

        Dictionary<string, ProductUnitType> byName = targetRows.ToDictionary(u => u.Name.Value);

        // NOT: Hedefteki birimlerin kimlikleri kaynak kimliklerle aynı olmadığı
        // için burada "hedef kimliği → kendisi" diye ön doldurma YAPILMAZ. Doğru
        // eşleme yalnızca kaynak satırları üzerinden, ada göre kurulur.
        int added = 0;
        foreach (NamedRow row in sourceRows)
        {
            if (byName.TryGetValue(row.Name, out ProductUnitType? already))
            {
                // Hedefteki birim cinsinin kimliği korunur; kaynak kimlik
                // ürünlerin birim cinsi FK'sini çözebilmek için eşlenir.
                map[row.Id] = already.Id.Value;
                continue;
            }

            var copy = new ProductUnitType(new Name(row.Name), true);
            targetContext.Set<ProductUnitType>().Add(copy);
            byName[row.Name] = copy;
            map[row.Id] = copy.Id.Value;
            added++;
        }

        return added;
    }

    private async Task<int> CopyTaxRatesAsync(
        ApplicationDbContext source,
        Dictionary<Guid, Guid> map,
        CancellationToken cancellationToken)
    {
        List<TaxRateRow> sourceRows = await source.Set<TaxRate>()
            .AsNoTracking()
            .Select(t => new TaxRateRow(t.Id, t.Name.Value, t.Rate))
            .ToListAsync(cancellationToken);

        List<TaxRate> targetRows = await targetContext.Set<TaxRate>().ToListAsync(cancellationToken);
        Dictionary<string, TaxRate> byName = targetRows.ToDictionary(t => t.Name.Value);

        int added = 0;
        foreach (TaxRateRow row in sourceRows)
        {
            if (byName.TryGetValue(row.Name, out TaxRate? already))
            {
                // Hedefteki KDV oranı korunur; yalnızca eşleme kurulur.
                map[row.Id] = already.Id.Value;
                continue;
            }

            // Hedefte olmayan bir oran için kaynağın GERÇEK oranı kopyalanır.
            // Sıfır atanırsa hedefte %0'lık bir KDV kaydı oluşur ve ürünler
            // yanlış vergi ile devredilmiş olur.
            var copy = new TaxRate(new Name(row.Name), row.Rate, true);
            targetContext.Set<TaxRate>().Add(copy);
            byName[row.Name] = copy;
            map[row.Id] = copy.Id.Value;
            added++;
        }

        return added;
    }

    private async Task<(int Added, int Skipped, List<Guid> AddedSourceIds)> CopyChartOfAccountsAsync(
        ApplicationDbContext source,
        Dictionary<Guid, Guid> map,
        CancellationToken cancellationToken)
    {
        // Seviye sırası: üst hesap (ParentId) alt hesaptan önce gelmelidir.
        // OrderBy(a => a.Code.Value) kaynak bağlamda SQL'e çevrilemiyor
        // (değer nesnesi alanı), bu yüzden sıralama bellekte yapılır.
        List<ChartOfAccount> items = await source.Set<ChartOfAccount>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        items = [.. items
            .OrderBy(a => a.Level)
            .ThenBy(a => a.Code.Value, StringComparer.Ordinal)];

        List<ChartOfAccount> targetRows = await targetContext.Set<ChartOfAccount>().ToListAsync(cancellationToken);
        Dictionary<string, ChartOfAccount> byCode = targetRows.ToDictionary(a => a.Code.Value);

        var addedSourceIds = new List<Guid>();
        int added = 0, skipped = 0;

        foreach (ChartOfAccount item in items)
        {
            Guid sourceId = item.Id.Value;
            string code = item.Code.Value;

            if (byCode.TryGetValue(code, out ChartOfAccount? already))
            {
                // Hedefteki hesaba dokunulmaz; yalnızca eşlemeye alınır.
                map[sourceId] = already.Id.Value;
                skipped++;
                continue;
            }

            var copy = new ChartOfAccount(
                new AccountCode(code),
                new Name(item.Name.Value),
                item.Level,
                item.Type);

            if (item.ParentId is { } parentSource && map.TryGetValue(parentSource.Value, out Guid parentTarget))
            {
                copy.SetParent(new IdentityId(parentTarget));
            }

            targetContext.Set<ChartOfAccount>().Add(copy);
            byCode[code] = copy;
            map[sourceId] = copy.Id.Value;
            addedSourceIds.Add(sourceId);
            added++;
        }

        return (added, skipped, addedSourceIds);
    }

    /// <summary>
    /// Yarı mamul / mamul bağlantıları hedefteki hesaplar eklendikten sonra
    /// kurulur: kaynak sırası bağımlılık garantisi vermez ve hedefte önceden
    /// bulunan hesapların bağlantıları bozulmamalıdır.
    /// </summary>
    private async Task ResolveProductionLinksAsync(
        ApplicationDbContext source,
        Dictionary<Guid, Guid> map,
        List<Guid> addedSourceIds,
        CancellationToken cancellationToken)
    {
        // Hesap planı seçenek kapalıysa ya da hiç yeni hesap eklenmediyse
        // bağlantı kurulmaz: hedefteki mevcut hesaplar değiştirilmemelidir.
        if (addedSourceIds.Count == 0)
        {
            return;
        }

        HashSet<Guid> addedSet = addedSourceIds.ToHashSet();

        List<ProductionLinkRow> links = await source.Set<ChartOfAccount>()
            .AsNoTracking()
            .Where(a => a.SemiFinishedAccountId != null || a.FinishedAccountId != null)
            .Select(a => new ProductionLinkRow(
                a.Id,
                a.SemiFinishedAccountId == null ? null : a.SemiFinishedAccountId.Value,
                a.FinishedAccountId == null ? null : a.FinishedAccountId.Value))
            .ToListAsync(cancellationToken);

        if (links.Count == 0)
        {
            return;
        }

        // Hedefte YALNIZCA bu adımda eklenen hesaplar güncellenir.
        List<Guid> targetIds = addedSet
            .Where(map.ContainsKey)
            .Select(sourceId => map[sourceId])
            .ToList();

        if (targetIds.Count == 0)
        {
            return;
        }

        Dictionary<Guid, ChartOfAccount> targets = await targetContext.Set<ChartOfAccount>()
            .Where(a => targetIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id.Value, a => a, cancellationToken);

        foreach (ProductionLinkRow link in links)
        {
            // Kaynak hesabı bu adımda eklenmemişse (hedefte zaten vardı) atlanır.
            if (addedSet.Contains(link.SourceId)
                && targets.TryGetValue(map[link.SourceId], out ChartOfAccount? account))
            {
                account.SetProductionLinks(
                    Translate(link.SemiFinishedSourceId, map),
                    Translate(link.FinishedSourceId, map));
            }
        }
    }

    private async Task<(int Added, int Skipped)> CopyCustomersAsync(
        ApplicationDbContext source,
        Dictionary<Guid, Guid> map,
        CancellationToken cancellationToken)
    {
        List<PartyRow> rows = await source.Set<Customer>()
            .AsNoTracking()
            .Select(c => new PartyRow(
                c.Id,
                c.Name.Value,
                c.TaxOffice.Value,
                c.TaxNumber.Value,
                c.Contact.PhoneNumber1,
                c.Contact.PhoneNumber2,
                c.Contact.Email,
                c.Address.City,
                c.Address.District,
                c.Address.FullAddress,
                c.Description.Value,
                c.IsActive))
            .ToListAsync(cancellationToken);

        List<Customer> targetRows = await targetContext.Set<Customer>().ToListAsync(cancellationToken);
        Dictionary<string, Customer> byTaxNumber = targetRows.ToDictionary(c => c.TaxNumber.Value);

        int added = 0, skipped = 0;

        foreach (PartyRow row in rows)
        {
            if (byTaxNumber.TryGetValue(row.TaxNumber, out Customer? already))
            {
                map[row.Id] = already.Id.Value;
                skipped++;
                continue;
            }

            var copy = new Customer(
                new Name(row.Name),
                new TaxOffice(row.TaxOffice),
                new TaxNumber(row.TaxNumber),
                new Contact(row.Phone1, row.Phone2, row.Email),
                new Address(row.City, row.District, row.FullAddress),
                new Description(row.Description),
                row.IsActive);

            targetContext.Set<Customer>().Add(copy);
            byTaxNumber[row.TaxNumber] = copy;
            map[row.Id] = copy.Id.Value;
            added++;
        }

        return (added, skipped);
    }

    private async Task<(int Added, int Skipped)> CopySuppliersAsync(
        ApplicationDbContext source,
        Dictionary<Guid, Guid> map,
        CancellationToken cancellationToken)
    {
        List<PartyRow> rows = await source.Set<Supplier>()
            .AsNoTracking()
            .Select(s => new PartyRow(
                s.Id,
                s.Name.Value,
                s.TaxOffice.Value,
                s.TaxNumber.Value,
                s.Contact.PhoneNumber1,
                s.Contact.PhoneNumber2,
                s.Contact.Email,
                s.Address.City,
                s.Address.District,
                s.Address.FullAddress,
                s.Description.Value,
                s.IsActive))
            .ToListAsync(cancellationToken);

        List<Supplier> targetRows = await targetContext.Set<Supplier>().ToListAsync(cancellationToken);
        Dictionary<string, Supplier> byTaxNumber = targetRows.ToDictionary(s => s.TaxNumber.Value);

        int added = 0, skipped = 0;

        foreach (PartyRow row in rows)
        {
            if (byTaxNumber.TryGetValue(row.TaxNumber, out Supplier? already))
            {
                map[row.Id] = already.Id.Value;
                skipped++;
                continue;
            }

            var copy = new Supplier(
                new Name(row.Name),
                new TaxOffice(row.TaxOffice),
                new TaxNumber(row.TaxNumber),
                new Contact(row.Phone1, row.Phone2, row.Email),
                new Address(row.City, row.District, row.FullAddress),
                new Description(row.Description),
                row.IsActive);

            targetContext.Set<Supplier>().Add(copy);
            byTaxNumber[row.TaxNumber] = copy;
            map[row.Id] = copy.Id.Value;
            added++;
        }

        return (added, skipped);
    }

    private async Task<(int Products, int Skipped, int Prices, int Photos)> CopyProductsAsync(
        ApplicationDbContext source,
        Dictionary<Guid, Guid> productMap,
        Dictionary<Guid, Guid> accountMap,
        Dictionary<Guid, Guid> unitMap,
        Dictionary<Guid, Guid> taxRateMap,
        bool includePrices,
        CancellationToken cancellationToken)
    {
        List<ProductRow> rows = await source.Set<Product>()
            .AsNoTracking()
            .Select(p => new ProductRow(
                p.Id,
                p.Name.Value,
                p.ProductCode.Value,
                p.Barcode.Value,
                p.QRCode.Value,
                p.MinimumProductLevel,
                p.TaxRateId.Value,
                p.WarehouseId.Value,
                p.CategoryId.Value,
                p.ProductUnitTypeId.Value,
                p.ChartOfAccountId == null ? null : p.ChartOfAccountId.Value,
                p.SemiFinishedProductId == null ? null : p.SemiFinishedProductId.Value,
                p.Description.Value,
                p.IsActive))
            .ToListAsync(cancellationToken);

        List<Product> targetRows = await targetContext.Set<Product>().ToListAsync(cancellationToken);
        Dictionary<string, Product> byCode = targetRows.ToDictionary(p => p.ProductCode.Value);

        // Yeni eklenen ürün, kaynak satırıyla birlikte tutulur: fiyat/görsel
        // kopyalama ikinci bir turda (kimlikler oluştuktan sonra) yapılır.
        List<(Product Entity, ProductRow Row)> created = [];
        int skipped = 0;

        foreach (ProductRow row in rows)
        {
            if (byCode.TryGetValue(row.ProductCode, out Product? already))
            {
                productMap[row.Id] = already.Id.Value;
                skipped++;
                continue;
            }

            var copy = new Product(
                new Name(row.Name),
                new ProductCode(row.ProductCode),
                new Barcode(row.Barcode),
                new QRCode(row.QRCode),
                row.MinimumProductLevel,
                new IdentityId(ResolveRequired(taxRateMap, row.TaxRateId, "KDV oranı")),
                new IdentityId(ResolveRequired(accountMap, row.WarehouseId, "depo hesabı")),
                new IdentityId(ResolveRequired(accountMap, row.CategoryId, "kategori hesabı")),
                new IdentityId(ResolveRequired(unitMap, row.ProductUnitTypeId, "birim cinsi")),
                Translate(row.ChartOfAccountSourceId, accountMap),
                new Description(row.Description),
                row.IsActive);

            targetContext.Set<Product>().Add(copy);
            byCode[row.ProductCode] = copy;
            productMap[row.Id] = copy.Id.Value;
            created.Add((copy, row));
        }

        await targetContext.SaveChangesAsync(cancellationToken);

        // Yarı mamul bağlantısı kendi kendine de referans verebilir; kayıtlar
        // eklendikten sonra kurulur.
        foreach ((Product entity, ProductRow row) in created)
        {
            if (row.SemiFinishedSourceId is { } semiSource
                && productMap.TryGetValue(semiSource, out Guid semiTarget))
            {
                entity.SetSemiFinishedProduct(new IdentityId(semiTarget));
            }
        }

        int prices = 0, photos = 0;

        if (created.Count > 0)
        {
            if (includePrices)
            {
                prices = await CopyProductPricesAsync(source, created, cancellationToken);
            }

            photos = await CopyProductPhotosAsync(source, created, cancellationToken);
        }

        return (created.Count, skipped, prices, photos);
    }

    private async Task<int> CopyProductPricesAsync(
        ApplicationDbContext source,
        List<(Product Entity, ProductRow Row)> created,
        CancellationToken cancellationToken)
    {
        // Fiyatın ProductId'si gölge (shadow) FK olduğu için projeksiyonda
        // EF.Property ile okunmalıdır. Gölge FK nullable olabilir; null
        // değer okunmaya çalışılırsa EF InvalidOperationException fırlatır.
        List<PriceRow> rows = await source.Set<ProductPrice>()
            .AsNoTracking()
            .Where(p => EF.Property<Guid?>(p, "ProductId") != null)
            .Select(p => new PriceRow(
                EF.Property<Guid>(p, "ProductId"),
                p.UnitPrice.Value,
                p.PriceType,
                p.StartDate,
                p.EndDate))
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        Dictionary<Guid, List<PriceRow>> bySourceProduct =
            rows.GroupBy(r => r.SourceProductId).ToDictionary(g => g.Key, g => g.ToList());

        int added = 0;

        foreach ((Product entity, ProductRow row) in created)
        {
            if (!bySourceProduct.TryGetValue(row.Id, out List<PriceRow>? group))
            {
                continue;
            }

            entity.ReplacePrices(group.Select(r =>
                new ProductPrice(new Price(r.UnitPrice), r.PriceType, r.StartDate, r.EndDate)));

            added += group.Count;
        }

        return added;
    }

    private async Task<int> CopyProductPhotosAsync(
        ApplicationDbContext source,
        List<(Product Entity, ProductRow Row)> created,
        CancellationToken cancellationToken)
    {
        // Photo tablosu çok amaçlıdır: "ProductId" gölge FK'sı ürüne bağlı
        // olmayan fotoğraflarda NULL'dır. EF.Property<Guid>(...) null bir
        // değerde InvalidOperationException fırlatır; bu yüzden nullable
        // okunur ve ürüne bağlı olmayanlar elenir.
        List<PhotoRow> rows = await source.Set<Photo>()
            .AsNoTracking()
            .Where(p => EF.Property<Guid?>(p, "ProductId") != null)
            .Select(p => new PhotoRow(
                EF.Property<Guid>(p, "ProductId"),
                p.FileName,
                p.ContentType,
                p.Path,
                p.IsDefault))
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        Dictionary<Guid, List<PhotoRow>> bySourceProduct =
            rows.GroupBy(r => r.SourceProductId).ToDictionary(g => g.Key, g => g.ToList());

        int added = 0;

        foreach ((Product entity, ProductRow row) in created)
        {
            if (!bySourceProduct.TryGetValue(row.Id, out List<PhotoRow>? group))
            {
                continue;
            }

            entity.ReplaceImages(
                group.Select(r => new Photo(r.FileName, r.ContentType, r.Path, r.IsDefault)));

            added += group.Count;
        }

        return added;
    }

    private async Task<(int Recipes, int Items)> CopyRecipesAsync(
        ApplicationDbContext source,
        Dictionary<Guid, Guid> productMap,
        CancellationToken cancellationToken)
    {
        if (productMap.Count == 0)
        {
            return (0, 0);
        }

        List<RecipeRow> recipes = await source.Set<Recipe>()
            .AsNoTracking()
            .Select(r => new RecipeRow(r.Id, r.ProductId.Value))
            .ToListAsync(cancellationToken);

        List<RecipeItemRow> items = await source.Set<RecipeItem>()
            .AsNoTracking()
            .Select(i => new RecipeItemRow(i.RecipeId.Value, i.ProductId.Value, i.Quantity))
            .ToListAsync(cancellationToken);

        List<Recipe> targetRows = await targetContext.Set<Recipe>().ToListAsync(cancellationToken);
        Dictionary<Guid, Recipe> byProduct = targetRows.ToDictionary(r => r.ProductId.Value);

        var recipeMap = new Dictionary<Guid, Guid>();
        HashSet<Guid> newlyCreatedRecipeIds = [];
        int recipeCount = 0;

        foreach (RecipeRow row in recipes)
        {
            if (!productMap.TryGetValue(row.SourceProductId, out Guid targetProductId))
            {
                continue;
            }

            if (byProduct.TryGetValue(targetProductId, out Recipe? already))
            {
                recipeMap[row.SourceId] = already.Id.Value;
                continue;
            }

            var copy = new Recipe(new IdentityId(targetProductId));
            targetContext.Set<Recipe>().Add(copy);
            recipeMap[row.SourceId] = copy.Id.Value;
            newlyCreatedRecipeIds.Add(copy.Id.Value);
            recipeCount++;
        }

        await targetContext.SaveChangesAsync(cancellationToken);

        int itemCount = 0;

        // Yalnızca yeni eklenen reçetelerin satırları kopyalanır; hedefteki
        // mevcut reçetelerin malzeme listesi korunur.
        foreach (RecipeItemRow row in items)
        {
            if (!recipeMap.TryGetValue(row.RecipeId, out Guid targetRecipeId)
                || !newlyCreatedRecipeIds.Contains(targetRecipeId)
                || !productMap.TryGetValue(row.SourceProductId, out Guid targetMaterialId))
            {
                continue;
            }

            var copy = new RecipeItem(new IdentityId(targetMaterialId), row.Quantity);
            copy.SetRecipe(new IdentityId(targetRecipeId));

            targetContext.Set<RecipeItem>().Add(copy);
            itemCount++;
        }

        return (recipeCount, itemCount);
    }

    // --------------------------------------------------------------- bakiyeler

    private async Task<int> WriteCurrentAccountBalancesAsync(
        ApplicationDbContext source,
        Dictionary<Guid, Guid> customerMap,
        Dictionary<Guid, Guid> supplierMap,
        DateOnly openingDate,
        string documentNo,
        CancellationToken cancellationToken)
    {
        List<CurrentAccountAggregate> rows = await ReadCurrentAccountAggregateAsync(source, cancellationToken);

        int added = 0;

        foreach (CurrentAccountAggregate row in rows)
        {
            decimal net = row.Debit - row.Credit;

            if (Math.Abs(net) <= MoneyTolerance)
            {
                continue;
            }

            bool isDebit = net > 0;
            decimal debit = isDebit ? net : 0m;
            decimal credit = isDebit ? 0m : -net;

            if (row.Type == CurrentAccountType.Customer)
            {
                if (row.SourceCustomerId is not { } sourceId
                    || !customerMap.TryGetValue(sourceId, out Guid targetId))
                {
                    continue;
                }

                targetContext.Set<CurrentAccountMovement>().Add(new CurrentAccountMovement(
                    CurrentAccountType.Customer,
                    new IdentityId(targetId),
                    null,
                    openingDate,
                    CurrentAccountMovementType.OpeningBalance,
                    documentNo,
                    debit,
                    credit,
                    new Description("Devir bakiyesi")));

                added++;
                continue;
            }

            if (row.SourceSupplierId is not { } supplierSourceId
                || !supplierMap.TryGetValue(supplierSourceId, out Guid targetSupplierId))
            {
                continue;
            }

            targetContext.Set<CurrentAccountMovement>().Add(new CurrentAccountMovement(
                CurrentAccountType.Supplier,
                null,
                new IdentityId(targetSupplierId),
                openingDate,
                CurrentAccountMovementType.OpeningBalance,
                documentNo,
                debit,
                credit,
                new Description("Devir bakiyesi")));

            added++;
        }

        return added;
    }

    private async Task<int> WriteStockBalancesAsync(
        ApplicationDbContext source,
        Dictionary<Guid, Guid> productMap,
        DateOnly openingDate,
        string documentNo,
        CancellationToken cancellationToken)
    {
        List<StockAggregate> rows = await ReadStockAggregateAsync(source, cancellationToken);

        int added = 0;

        foreach (StockAggregate row in rows)
        {
            if (Math.Abs(row.Quantity) <= StockTolerance
                || !productMap.TryGetValue(row.SourceProductId, out Guid targetId))
            {
                continue;
            }

            bool isInput = row.Quantity > 0;

            // Ortalama birim maliyet: FIFO/LIFO katmanları yeni yıla taşınmadığı
            // için devir değeri tek bir giriş olarak yazılır.
            decimal unitPrice = Math.Round(row.Value / row.Quantity, 4);

            targetContext.Set<ProductMovement>().Add(new ProductMovement(
                new IdentityId(targetId),
                isInput ? ProductMovementType.Input : ProductMovementType.Output,
                Math.Abs(row.Quantity),
                new Price(unitPrice),
                openingDate,
                documentNo,
                new Description("Devir / Transfer"),
                ProductMovementReason.Transfer));

            added++;
        }

        return added;
    }

    private async Task<int> WriteChartBalancesAsync(
        ApplicationDbContext source,
        Dictionary<Guid, Guid> accountMap,
        DateOnly openingDate,
        string documentNo,
        CancellationToken cancellationToken)
    {
        if (accountMap.Count == 0)
        {
            return 0;
        }

        List<ChartAggregate> rows = await ReadChartAggregateAsync(source, cancellationToken);

        int added = 0;

        foreach (ChartAggregate row in rows)
        {
            if (!accountMap.TryGetValue(row.SourceAccountId, out Guid targetId))
            {
                continue;
            }

            decimal debit = Math.Round(row.Debit, 2);
            decimal credit = Math.Round(row.Credit, 2);

            if (debit <= MoneyTolerance && credit <= MoneyTolerance)
            {
                continue;
            }

            targetContext.Set<ChartOfAccountLedger>().Add(new ChartOfAccountLedger(
                new IdentityId(targetId),
                debit,
                credit,
                DevirSourceType,
                null));

            added++;
        }

        return added;
    }

    // ----------------------------------------------------------------- yardımcı

    /// <summary>
    /// Kopyalama kapalıyken hedefteki mevcut kayıtları eşlemeye alır. Hedefte
    /// bulunamayan kayıtlar sessizce atlanır ve bakiyeleri de aktarılamaz.
    /// </summary>
    private async Task<int> MapExistingAsync<TEntity>(
        ApplicationDbContext source,
        Dictionary<Guid, Guid> map,
        CancellationToken cancellationToken)
        where TEntity : Entity
    {
        List<(Guid Id, string Key)> sourceRows = await ReadNaturalKeysAsync<TEntity>(source, cancellationToken);
        if (sourceRows.Count == 0)
        {
            return 0;
        }

        List<(Guid Id, string Key)> targetRows = await ReadNaturalKeysAsync<TEntity>(targetContext, cancellationToken);
        Dictionary<string, Guid> byKey = targetRows.ToDictionary(r => r.Key, r => r.Id);

        int matched = 0;
        foreach ((Guid sourceId, string key) in sourceRows)
        {
            if (byKey.TryGetValue(key, out Guid targetId))
            {
                map[sourceId] = targetId;
                matched++;
            }
        }

        return matched;
    }

    /// <summary>
    /// Doğal anahtar (veritabanında benzersiz) çiftlerini okur. Sorgu tüm
    /// satırları belleğe alıp eşleme yaparak LINQ çeviri riskinden kaçınır.
    /// </summary>
    private static async Task<List<(Guid Id, string Key)>> ReadNaturalKeysAsync<TEntity>(
        ApplicationDbContext context,
        CancellationToken cancellationToken)
        where TEntity : Entity
    {
        List<TEntity> rows = await context.Set<TEntity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return [.. rows.Select(e => (e.Id.Value, NaturalKeyOf(e)))];
    }

    private static string NaturalKeyOf(Entity entity) => entity switch
    {
        ChartOfAccount a => a.Code.Value,
        Product p => p.ProductCode.Value,
        Customer c => c.TaxNumber.Value,
        Supplier s => s.TaxNumber.Value,
        _ => throw new NotSupportedException(
            $"{entity.GetType().Name} için devir doğal anahtarı tanımlı değil.")
    };

    private static IdentityId? Translate(Guid? sourceId, Dictionary<Guid, Guid> map)
        => sourceId is { } id && map.TryGetValue(id, out Guid targetId)
            ? new IdentityId(targetId)
            : null;

    private static Guid ResolveRequired(Dictionary<Guid, Guid> map, Guid sourceId, string label)
        => map.TryGetValue(sourceId, out Guid targetId)
            ? targetId
            : throw new InvalidOperationException(
                $"Devir için gerekli '{label}' kaydı hedefte bulunamadı (Id: {sourceId}).");

    private sealed record TargetSelection(IdentityId CompanyId, int Year, string DatabaseName);

    private sealed record SourceYearRef(int Year, string DatabaseName);

    private sealed record CurrentAccountAggregate(
        CurrentAccountType Type,
        Guid? SourceCustomerId,
        Guid? SourceSupplierId,
        decimal Debit,
        decimal Credit);

    private sealed record StockAggregate(Guid SourceProductId, decimal Quantity, decimal Value);

    private sealed record ChartAggregate(Guid SourceAccountId, decimal Debit, decimal Credit);

    private sealed record NamedRow(Guid Id, string Name);

    private sealed record TaxRateRow(Guid Id, string Name, decimal Rate);

    private sealed record ProductionLinkRow(
        Guid SourceId,
        Guid? SemiFinishedSourceId,
        Guid? FinishedSourceId);

    private sealed record PartyRow(
        Guid Id,
        string Name,
        string TaxOffice,
        string TaxNumber,
        string Phone1,
        string Phone2,
        string Email,
        string City,
        string District,
        string FullAddress,
        string Description,
        bool IsActive);

    private sealed record ProductRow(
        Guid Id,
        string Name,
        string ProductCode,
        string? Barcode,
        string? QRCode,
        decimal? MinimumProductLevel,
        Guid TaxRateId,
        Guid WarehouseId,
        Guid CategoryId,
        Guid ProductUnitTypeId,
        Guid? ChartOfAccountSourceId,
        Guid? SemiFinishedSourceId,
        string Description,
        bool IsActive);

    private sealed record PriceRow(
        Guid SourceProductId,
        decimal UnitPrice,
        ProductPriceType PriceType,
        DateOnly StartDate,
        DateOnly? EndDate);

    private sealed record PhotoRow(
        Guid SourceProductId,
        string FileName,
        string ContentType,
        string Path,
        bool IsDefault);

    private sealed record RecipeRow(Guid SourceId, Guid SourceProductId);

    private sealed record RecipeItemRow(Guid RecipeId, Guid SourceProductId, decimal Quantity);
}
