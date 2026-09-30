namespace Cost.Accounting.Automation.Application.Services;

/// <summary>
/// Master veritabanındaki şirketlerin yalnızca kimlik/ad bilgisini okur.
/// Denetim (CreatedBy) birleştirmesi içermez; bu sayede oturum açılmadan,
/// yani giriş ekranında da güvenle kullanılabilir.
/// </summary>
public interface IMasterCompanyNameLookup
{
    Task<List<MasterCompanyInfo>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, string>> GetNamesAsync(
        IReadOnlyCollection<Guid> companyIds,
        CancellationToken cancellationToken = default);
}

public sealed record MasterCompanyInfo(Guid Id, string Name, string? TaxNumber);
