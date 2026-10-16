using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.DirectProcurement;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

/// <summary>
/// Doğrudan temin dosyaları için EF Core repository.
/// </summary>
internal sealed class DirectProcurementFileRepository
    : AuditableRepository<DirectProcurementFile, ApplicationDbContext>,
      IDirectProcurementFileRepository
{
    public DirectProcurementFileRepository(
        ApplicationDbContext context,
        MasterDbContext masterContext) : base(context, masterContext)
    {
    }

    /// <summary>
    /// Dosyanın altında teklif, kazanan firma veya bütçe kaydı varsa dosya
    /// silinemez: bu kayıtlar dosyanın mali izini oluşturur ve sessizce
    /// silinmeleri muhasebe kaydını koparır.
    /// </summary>
    public async Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken cancellationToken = default)
    {
        List<Guid?> keys = [.. fileIds.Select(id => (Guid?)id)];
        if (keys.Count == 0)
        {
            return DeletionCheck.Empty;
        }

        // Kimlikler önce belleğe alınır, sonra Guid'e çevrilir: IdentityId
        // bir değer dönüştürücüsüdür ve LINQ ifadesi içinde .Value'ya
        // erişmek SQL'e güvenilir biçimde çevrilmez.
        List<IdentityId> quotedFileIds = await this.Context.Set<ProcurementQuotedFirm>()
            .Where(q => keys.Contains(q.DirectProcurementFileId))
            .Select(q => q.DirectProcurementFileId)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<IdentityId> awardedFileIds = await this.Context.Set<ProcurementAwardedFirm>()
            .Where(a => keys.Contains(a.DirectProcurementFileId))
            .Select(a => a.DirectProcurementFileId)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<IdentityId> budgetFileIds = await this.Context.Set<DirectProcurementBudgetSelection>()
            .Where(s => keys.Contains(s.DirectProcurementFileId))
            .Select(s => s.DirectProcurementFileId)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<Guid> movementIds = [.. quotedFileIds
            .Concat(awardedFileIds)
            .Select(id => id.Value)
            .Distinct()];

        // Teklif veya kazanan firma kaydı olan dosyalar "hareket" sayılır; yalnızca
        // bütçe seçimi olanlar ilişkili kayıt sayılır ve uyarıyla silinir.
        List<Guid> relatedIds = [.. budgetFileIds
            .Select(id => id.Value)
            .Distinct()
            .Except(movementIds)];

        return new DeletionCheck(movementIds, relatedIds);
    }

    public async Task<DirectProcurementFile?> GetByIdWithDetailsAsync(
        IdentityId id,
        CancellationToken cancellationToken = default)
    {
        IQueryable<DirectProcurementFile> query = this.Context
            .Set<DirectProcurementFile>()
            .AsSplitQuery()   // alt koleksiyonlar için ayrı sorgular (cartesian çarpımı önler)
            .Include(f => f.Supplier)
            .Include(f => f.Items)
            .Include(f => f.HistoryPurchases)
            .Include(f => f.QuotedFirms)
            .Include(f => f.ApprovedFirms)
            .Include(f => f.AwardedFirms)
                .ThenInclude(a => a.Accounts)
            .Include(f => f.TeamMembers)
            .Include(f => f.BudgetSelections);

        return await query.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }
}