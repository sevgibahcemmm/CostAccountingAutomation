using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.AccountingYears.ValueObjects;

namespace Cost.Accounting.Automation.Application.Services;

/// <summary>
/// Oturumda açık olan şirket + mali yıl bilgisini tutar ve yıl veritabanının
/// bağlantı dizesini bu seçime göre üretir.
/// Seçim yapılmadan önce şablon bağlantı dizesi döner; bu sayede giriş ekranı
/// ve yıl seçim ekranı yıl veritabanına ihtiyaç duymadan çalışabilir.
/// </summary>
public interface IAccountingDbSelector
{
    /// <summary>Şirket + yıl seçimi yapıldı mı?</summary>
    bool HasSelection { get; }

    IdentityId? CompanyId { get; }

    Year? Year { get; }

    /// <summary>Seçili yılın veritabanı adı.</summary>
    string? DatabaseName { get; }

    /// <summary>
    /// Master (merkezi) veritabanının adı. Yıl veritabanı adı önerilirken
    /// bu adla çakışan adların elenmesi için gerekir.
    /// </summary>
    string MasterDatabaseName { get; }

    void Select(IdentityId companyId, Year year, string databaseName);

    void Clear();

    /// <summary>
    /// Seçime göre yıl veritabanının bağlantı dizesi; seçim yoksa şablon dize döner.
    /// </summary>
    string GetConnectionString();

    /// <summary>
    /// Verilen veritabanı adına yönelen bağlantı dizesini üretir. Yıl
    /// veritabanı oluşturulurken seçim henüz yapılmadığı için bu yol kullanılır.
    /// </summary>
    string BuildConnectionString(string? databaseName);
}
