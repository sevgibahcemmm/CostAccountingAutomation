using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Services;

/// <summary>
/// Master veritabanından şirket kimlik/ad bilgisini okur. Denetim birleştirmesi
/// içermediği için giriş ekranı gibi oturumsuz ekranlarda da kullanılabilir.
/// </summary>
internal sealed class MasterCompanyNameLookup(MasterDbContext context) : IMasterCompanyNameLookup
{
    public Task<List<MasterCompanyInfo>> GetAllAsync(CancellationToken cancellationToken = default)
        => context.Companies
            .AsNoTrackingWithIdentityResolution()
            .OrderBy(c => c.Name.Value)
            .Select(c => new MasterCompanyInfo(
                c.Id.Value,
                c.Name.Value,
                c.TaxNumber.Value))
            .ToListAsync(cancellationToken);

    public async Task<Dictionary<Guid, string>> GetNamesAsync(
        IReadOnlyCollection<Guid> companyIds,
        CancellationToken cancellationToken = default)
    {
        if (companyIds.Count == 0)
        {
            return [];
        }

        // Id, value converter ile Guid'a eşlenmiş bir alan olduğu için filtre
        // koşulunda c.Id.Value kullanılamaz (EF çeviremez). Aynı CLR tipinde
        // karşılaştırma yapılır, dönüşüm EF tarafından uygulanır.
        List<IdentityId> ids = companyIds.Select(id => new IdentityId(id)).ToList();

        return await context.Companies
            .AsNoTrackingWithIdentityResolution()
            .Where(c => ids.Contains(c.Id))
            .Select(c => new { CompanyId = c.Id.Value, CompanyName = c.Name.Value })
            .ToDictionaryAsync(x => x.CompanyId, x => x.CompanyName, cancellationToken);
    }
}
