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
    /// <param name="progress">
    /// Yalnızca alt adımları (hesap planı, birim/KDV, sanal kayıtlar) bildirmek
    /// için kullanılır. Opsiyoneldir; verilmezse bildirim yapılmaz.
    /// </param>
    Task<AccountingYearProvisionResult> EnsureDatabaseAsync(
        IdentityId companyId,
        int year,
        string databaseName,
        CancellationToken cancellationToken = default,
        IProgress<DatabaseProvisionProgress>? progress = null);
}

/// <summary>Yıl veritabanı hazırlama sonucu.</summary>
public sealed record AccountingYearProvisionResult
{
    public required string DatabaseName { get; init; }

    /// <summary>Veritabanı bu çağrıda yeni mi oluşturuldu?</summary>
    public required bool Created { get; init; }

    /// <summary>Uygulanan migration sayısı.</summary>
    public required int AppliedMigrationCount { get; init; }

    /// <summary>Bu çağrıda tohumlanan hesap planı satırı sayısı.</summary>
    public int SeededChartOfAccountCount { get; init; }

    /// <summary>Bu çağrıda tohumlanan birim cinsi ve KDV oranı sayısı.</summary>
    public int SeededReferenceCount { get; init; }

    /// <summary>Bu çağrıda tohumlanan müşteri ve tedarikçi sayısı.</summary>
    public int SeededSampleRecordCount { get; init; }
}
