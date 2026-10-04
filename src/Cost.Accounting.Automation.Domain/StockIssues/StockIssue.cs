using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Domain.StockIssues;

public enum StockIssueType
{
    Consumption = 1,
    AtelierTransfer = 2
}

/// <summary>
/// Stok çıkışı/tüketim belgesinin durumu. Belge her zaman TASLAK olarak
/// kaydedilir; stok ve yevmiye hareketleri ancak ONAYDA oluşur.
/// </summary>
public enum StockIssueStatus : byte
{
    Draft = 1,
    Approved = 2
}

public sealed class StockIssue : Entity, IHardDeletable
{
    private readonly List<StockIssueLine> _lines = [];

    private StockIssue()
    {
    }

    public StockIssue(
        StockIssueType issueType,
        string documentNumber,
        DateOnly date,
        IdentityId sourceWarehouseId,
        IdentityId targetAccountId,
        StockCostingMethod costingMethod,
        Description description)
    {
        IssueType = issueType;
        DocumentNumber = documentNumber;
        Date = date;
        SourceWarehouseId = sourceWarehouseId;
        TargetAccountId = targetAccountId;
        CostingMethod = costingMethod;
        Description = description;
        ResolveDuplicateKey();
    }

    public static string? BuildDuplicateKey(string documentNumber, StockIssueType issueType)
        => DuplicateKeyRule.From(documentNumber, ((int)issueType).ToString());

    public void ResolveDuplicateKey()
        => SetDuplicateKey(BuildDuplicateKey(DocumentNumber, IssueType));

    public StockIssueType IssueType { get; private set; }
    public string DocumentNumber { get; private set; } = default!;
    public DateOnly Date { get; private set; }

    /// <summary>
    /// Taslak kaydedilen belge stoğu DÜŞÜRMEZ. Atölye transferi veya tüketim
    /// onaylanana kadar ürün miktarı "bekleyen" olarak görünür, maliyet
    /// pusulasında tüketilemez.
    /// </summary>
    public StockIssueStatus Status { get; private set; } = StockIssueStatus.Draft;

    public IdentityId SourceWarehouseId { get; private set; } = default!;
    public ChartOfAccount? SourceWarehouse { get; private set; }

    public IdentityId TargetAccountId { get; private set; } = default!;
    public ChartOfAccount? TargetAccount { get; private set; }

    public StockCostingMethod CostingMethod { get; private set; }
    public Description Description { get; private set; } = default!;

    public IReadOnlyCollection<StockIssueLine> Lines => _lines;

    public void SetDescription(Description description) => Description = description;

    /// <summary>
    /// Tüketim/transfer belgelerinde maliyetlendirme yöntemi kullanıcı seçimine
    /// açık değildir; daima FIFO uygulanır. Yeniden oluşturulan (restore edilen)
    /// bir belge eski LIFO değerini taşıyabildiği için yöntem burada sıfırlanır.
    /// </summary>
    public void UseFifoCosting() => CostingMethod = StockCostingMethod.Fifo;

    public void ReplaceLines(IEnumerable<StockIssueLine> lines)
    {
        if (Status == StockIssueStatus.Approved)
        {
            throw new InvalidOperationException("Onaylanmış belge satırları değiştirilemez.");
        }

        _lines.Clear();
        _lines.AddRange(lines);
    }

    /// <summary>
    /// Belgeyi onaylar ve stok/yevmiye hareketlerinin üretilmesini sağlar.
    /// </summary>
    public void Approve()
    {
        if (Status == StockIssueStatus.Approved)
        {
            throw new InvalidOperationException("Belge zaten onaylanmış durumda.");
        }

        Status = StockIssueStatus.Approved;
    }
}
