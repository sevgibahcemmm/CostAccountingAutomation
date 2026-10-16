using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
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
    IChartOfAccountRepository chartOfAccountRepository,
    IAuditUserNameResolver auditUserNames) : IRequestHandler<ConsumptionUnitGetAllQuery, IQueryable<ConsumptionUnitDto>>
{
    public async Task<IQueryable<ConsumptionUnitDto>> Handle(ConsumptionUnitGetAllQuery request, CancellationToken cancellationToken)
    {
        // Tüketicim birimleri hesap planının çok küçük bir dilimi (yüzde bir
        // civarı). Tip filtresi veritabanında uygulanır: eskiden 6.000'den fazla
        // hesap belleğe alınıp filtreleniyordu.
        IQueryable<ChartOfAccount> source = request.OnlyDeleted
            ? chartOfAccountRepository.GetAllIncludingDeleted().Where(i => i.IsDeleted)
            : chartOfAccountRepository.GetAllActive();

        IQueryable<ConsumptionUnitDto> query = source
            .Where(i => i.Type == ChartOfAccountType.ConsumptionUnit)
            .Select(s => new ConsumptionUnitDto
            {
                Id = s.Id,
                Code = s.Code.Value,
                Name = s.Name.Value,

                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                IsActive = s.IsActive,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy == null ? null : s.UpdatedBy.Value,
            });

        return await query.ToAuditedListAsync(auditUserNames, cancellationToken);
    }
}
