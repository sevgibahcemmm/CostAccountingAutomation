using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.AccountingYears.ValueObjects;
using Microsoft.Data.SqlClient;

namespace Cost.Accounting.Automation.Infrastructure.Services;

/// <summary>
/// Seçili şirket + mali yıl bilgisini tutar. <see cref="Selection"/> değişmez
/// (immutable) bir kayıt olduğu için okuma anında tutarlı bir anlık görüntü
/// alınır ve referans ataması thread-safe'dir.
/// </summary>
public sealed class AccountingDbSelector : IAccountingDbSelector
{
    private readonly string _templateConnectionString;
    private Selection? _selection;

    public AccountingDbSelector(string templateConnectionString, string masterDatabaseName)
    {
        _templateConnectionString = templateConnectionString;
        MasterDatabaseName = masterDatabaseName;
    }

    public bool HasSelection => _selection is not null;

    public IdentityId? CompanyId => _selection?.CompanyId;

    public Year? Year => _selection?.Year;

    public string? DatabaseName => _selection?.DatabaseName;

    public string MasterDatabaseName { get; }

    public void Select(IdentityId companyId, Year year, string databaseName)
    {
        _selection = new Selection(companyId, year, databaseName);
    }

    public void Clear() => _selection = null;

    public string GetConnectionString() => BuildConnectionString(_selection?.DatabaseName);

    /// <summary>
    /// Verilen veritabanı adına yönelen bağlantı dizesini üretir. Seçim
    /// yapılmadan önce şablon dize kullanılır; şablon "master" kataloğuna
    /// baktığı için yıl veritabanı yanlışlıkla oluşturulamaz.
    /// </summary>
    public string BuildConnectionString(string? databaseName)
    {
        var builder = new SqlConnectionStringBuilder(_templateConnectionString);

        if (!string.IsNullOrWhiteSpace(databaseName))
        {
            builder.InitialCatalog = databaseName;
        }

        return builder.ConnectionString;
    }

    private sealed record Selection(IdentityId CompanyId, Year Year, string DatabaseName);
}
