using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class ProductMovementRepository : AuditableRepository<ProductMovement, ApplicationDbContext>, IProductMovementRepository
{
    public ProductMovementRepository(ApplicationDbContext context, MasterDbContext masterContext) : base(context, masterContext)
    {
    }

    /// <summary>
    /// <c>GetAllWithAudit</c> sonucu belleğe materyalize edildiği için navigasyon
    /// özellikleri ancak burada <c>Include</c> edilirse dolar. Aksi hâlde liste
    /// DTO'sunda ürün kodu, ürün adı, barkod, birim ve depo alanları sessizce
    /// boş kalır.
    ///
    /// Yalnızca referans navigasyonları kullanılır; koleksiyon içermediği için
    /// <c>AsSplitQuery</c> gerekmez.
    /// </summary>
    protected override IQueryable<ProductMovement> ApplyDetailIncludes(IQueryable<ProductMovement> query)
        => query
            .Include(m => m.Product).ThenInclude(p => p!.Warehouse)
            .Include(m => m.Product).ThenInclude(p => p!.ProductUnitType);

    public async Task<List<ProductMovementQueryResult>> GetByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
    {
        HashSet<IdentityId> ids = productIds.Select(id => new IdentityId(id)).ToHashSet();
        if (ids.Count == 0)
        {
            return [];
        }

        return await this.Context.Set<ProductMovement>()
            .AsNoTracking()
            .Where(m => ids.Contains(m.ProductId))
            .Select(m => new ProductMovementQueryResult(
                m.ProductId.Value,
                m.Id.Value,
                m.MovementType,
                m.Reason,
                m.Quantity,
                m.UnitPrice == null ? null : m.UnitPrice.Value,
                m.Date,
                m.ReferenceNo,
                m.Description.Value,
                m.InvoiceId == null ? null : m.InvoiceId.Value,
                m.StockIssueId == null ? null : m.StockIssueId.Value))
            .ToListAsync(cancellationToken);
    }
}