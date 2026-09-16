using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

public sealed class ConsumptionUnitDto : EntityDto
{
    [Column("Kod", Order = 5, Width = 120)]
    public string Code { get; set; } = default!;

    [Column("Tüketim Birimi", Order = 10, Width = 280)]
    public string Name { get; set; } = default!;
}

[Permission("chartofaccount:view")]
public sealed record ConsumptionUnitGetAllQuery(
    bool OnlyDeleted = false) : IRequest<IQueryable<ConsumptionUnitDto>>
{
    public ConsumptionUnitGetAllQuery() : this(false) { }
}

internal sealed class ConsumptionUnitGetAllQueryHandler(
    IChartOfAccountRepository chartOfAccountRepository) : IRequestHandler<ConsumptionUnitGetAllQuery, IQueryable<ConsumptionUnitDto>>
{
    public Task<IQueryable<ConsumptionUnitDto>> Handle(ConsumptionUnitGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EntityWithAuditDto<ChartOfAccount>> source = request.OnlyDeleted
            ? chartOfAccountRepository.GetAllWithAuditIncludingDeleted().Where(i => i.Entity.IsDeleted)
            : chartOfAccountRepository.GetAllWithAudit();

        IQueryable<ConsumptionUnitDto> query = source
            .Where(i => i.Entity.Type == ChartOfAccountType.ConsumptionUnit)
            .Select(s => new ConsumptionUnitDto
            {
                Id = s.Entity.Id,
                Code = s.Entity.Code.Value,
                Name = s.Entity.Name.Value,

                CreatedAt = s.Entity.CreatedAt,
                CreatedBy = s.Entity.CreatedBy,
                IsActive = s.Entity.IsActive,
                UpdatedAt = s.Entity.UpdatedAt,
                UpdatedBy = s.Entity.UpdatedBy == null ? null : s.Entity.UpdatedBy.Value,
                CreatedFullName = s.CreatedUser.FullName.Value,
                UpdatedFullName = s.UpdatedUser == null ? null : s.UpdatedUser.FullName.Value
            });

        return Task.FromResult(query);
    }
}
