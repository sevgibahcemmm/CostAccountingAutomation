using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips.CostSlipItems;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.CostSlips;

[Permission("costslip:view")]
public sealed record CostSlipGetAllQuery(
    CostSlipType? CostSlipType = null,
    bool OnlyDeleted = false,
    CostSlipStatus? Status = null) : IRequest<IQueryable<CostSlipListDto>>
{
    public CostSlipGetAllQuery() : this(null, false, null) { }
}

internal sealed class CostSlipGetAllQueryHandler(
    ICostSlipRepository costSlipRepository) : IRequestHandler<CostSlipGetAllQuery, IQueryable<CostSlipListDto>>
{
    /// <summary>
    /// Ana pusula kayıtlarını ve satırlarını <b>iki ayrı ve basit</b> sorguda
    /// getirir.
    /// </summary>
    /// <remarks>
    /// Satırlar önceden ana projeksiyonun içinde
    /// (<c>CostSlipItems = ....ToList()</c>) yükleniyordu. Bu, EF'in bölme
    /// (split) sorgusu üretmesine yol açıyor, ana kayıtlarla satırların
    /// kartesian çarpımını içeren 5300 karakterlik bir <c>JOIN</c> oluşuyor
    /// ve liste 25 saniye sürüyordu. Aynı SQL'in veritabanında 15 ms'de
    /// bittiği ölçülmüştü. Satırları ayrı sorguya taşımak bu sorguyu tamamen
    /// ortadan kaldırır.
    /// </remarks>
    public async Task<IQueryable<CostSlipListDto>> Handle(CostSlipGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EntityWithAuditDto<CostSlip>> source = request.OnlyDeleted
            ? costSlipRepository.GetAllWithAuditIncludingDeleted().Where(i => i.Entity.IsDeleted)
            : costSlipRepository.GetAllWithAudit();

        if (request.CostSlipType.HasValue)
        {
            source = source.Where(i => i.Entity.CostSlipType == request.CostSlipType.Value);
        }

        if (request.Status.HasValue)
        {
            source = source.Where(i => i.Entity.Status == request.Status.Value);
        }

        List<CostSlipListDto> slips = source.MapTo().ToList();

        if (slips.Count > 0)
        {
            IdentityId[] slipIds = slips.Select(x => new IdentityId(x.Id)).ToArray();
            var itemsBySlip = (await costSlipRepository.GetItemsAsync(slipIds, cancellationToken))
                .GroupBy(x => x.CostSlipId.Value)
                .ToDictionary(x => x.Key, x => x.MapToList());

            foreach (CostSlipListDto slip in slips)
            {
                slip.CostSlipItems = itemsBySlip.TryGetValue(slip.Id, out List<CostSlipItemDto>? items)
                    ? items
                    : [];
            }
        }

        return slips.AsQueryable();
    }
}