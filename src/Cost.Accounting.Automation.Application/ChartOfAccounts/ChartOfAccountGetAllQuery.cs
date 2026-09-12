using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

[Permission("chartofaccount:view")]
public sealed record ChartOfAccountGetAllQuery() : IRequest<IQueryable<ChartOfAccountDto>>;

internal sealed class ChartOfAccountGetAllQueryHandler(
    IChartOfAccountRepository chartOfAccountRepository) : IRequestHandler<ChartOfAccountGetAllQuery, IQueryable<ChartOfAccountDto>>
{
    public Task<IQueryable<ChartOfAccountDto>> Handle(ChartOfAccountGetAllQuery request, CancellationToken cancellationToken)
        => Task.FromResult(chartOfAccountRepository.GetAllWithAudit().MapTo());
}