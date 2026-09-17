using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
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
    public Task<IQueryable<CostSlipListDto>> Handle(CostSlipGetAllQuery request, CancellationToken cancellationToken)
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

        return Task.FromResult(source.MapTo().AsQueryable());
    }
}