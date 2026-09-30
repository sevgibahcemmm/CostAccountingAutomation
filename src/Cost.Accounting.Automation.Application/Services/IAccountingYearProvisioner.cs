using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Application.Services;

/// <summary>
/// Seçilen şirket + mali yıl için iş veritabanını hazırlar: veritabanı yoksa
/// oluşturur, bekleyen migration'ları uygular ve yıl başlangıcında gereken
/// temel kayıtları (birim, KDV oranı, müşteri/tedarikçi örnekleri) tohumlar.
/// </summary>
public interface IAccountingYearProvisioner
{
    /// <summary>
    /// <paramref name="databaseName"/> veritabanının var olduğundan ve güncel
    /// şemadan olduğundan emin olur.
    /// </summary>
    Task<AccountingYearProvisionResult> EnsureDatabaseAsync(
        IdentityId companyId,
        int year,
        string databaseName,
        CancellationToken cancellationToken = default);
}

/// <summary>Yıl veritabanı hazırlama sonucu.</summary>
public sealed record AccountingYearProvisionResult
{
    public required string DatabaseName { get; init; }

    /// <summary>Veritabanı bu çağrıda yeni mi oluşturuldu?</summary>
    public required bool Created { get; init; }

    /// <summary>Uygulanan migration sayısı.</summary>
    public required int AppliedMigrationCount { get; init; }
}
