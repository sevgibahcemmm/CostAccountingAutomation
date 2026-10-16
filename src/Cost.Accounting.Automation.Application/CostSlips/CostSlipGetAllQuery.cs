using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips.CostSlipItems;
using Microsoft.EntityFrameworkCore;
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
    ICostSlipRepository costSlipRepository,
    IAuditUserNameResolver auditUserNames) : IRequestHandler<CostSlipGetAllQuery, IQueryable<CostSlipListDto>>
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
        IQueryable<CostSlip> source = request.OnlyDeleted
            ? costSlipRepository.GetAllIncludingDeleted().Where(i => i.IsDeleted)
            : costSlipRepository.GetAllActive();

        if (request.CostSlipType is CostSlipType slipType)
        {
            source = source.Where(i => i.CostSlipType == slipType);
        }

        if (request.Status is CostSlipStatus status)
        {
            source = source.Where(i => i.Status == status);
        }

        List<CostSlipListDto> slips = await source
            .MapTo()
            .ToListAsync(cancellationToken);

        await auditUserNames.FillAsync(slips, cancellationToken);

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