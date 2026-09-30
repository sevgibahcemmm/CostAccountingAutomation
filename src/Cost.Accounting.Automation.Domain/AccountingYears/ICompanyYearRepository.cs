using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.AccountingYears;

/// <summary>
/// Yıl kayıtları master veritabanında tutulduğu için bu repository
/// <see cref="AuditableRepository{TEntity}"/> kalıbını kullanmaz; denetim
/// (CreatedBy/UpdatedBy) alanlarını kullanıcıyla birleştiren inner join
/// yıl veritabanında geçerli olmadığından bilinçli olarak arayüz dışı bırakıldı.
/// </summary>
public interface ICompanyYearRepository
{
    Task<List<CompanyYear>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<CompanyYear>> GetAllByCompanyAsync(
        IdentityId companyId,
        CancellationToken cancellationToken = default);

    /// <summary>Kapatılmamış yıllar; yıl açma sihirbazında varsayılan seçim listelenir.</summary>
    Task<List<CompanyYear>> GetAllOpenAsync(CancellationToken cancellationToken = default);

    Task<CompanyYear?> GetByIdAsync(IdentityId id, CancellationToken cancellationToken = default);

    Task<CompanyYear?> GetByCompanyAndYearAsync(
        IdentityId companyId,
        int year,
        CancellationToken cancellationToken = default);

    /// <summary>Veritabanı adı üzerinden kayıt arar (yıl DB'sine bağlanırken kullanılır).</summary>
    Task<CompanyYear?> GetByDatabaseNameAsync(
        string databaseName,
        CancellationToken cancellationToken = default);

    Task<bool> DatabaseNameExistsAsync(string databaseName, CancellationToken cancellationToken = default);

    Task AddAsync(CompanyYear companyYear, CancellationToken cancellationToken = default);

    void Update(CompanyYear companyYear);
}
