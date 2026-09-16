using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.StockIssues;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.StockIssues;

[Permission("stock_issue:view")]
public sealed record StockIssueGetAllQuery(
    StockIssueType? IssueType = null,
    bool OnlyDeleted = false) : IRequest<IQueryable<StockIssueListDto>>
{
    public StockIssueGetAllQuery() : this(null, false) { }
}

internal sealed class StockIssueGetAllQueryHandler(
    IStockIssueRepository stockIssueRepository) : IRequestHandler<StockIssueGetAllQuery, IQueryable<StockIssueListDto>>
{
    public Task<IQueryable<StockIssueListDto>> Handle(StockIssueGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EntityWithAuditDto<StockIssue>> source = request.OnlyDeleted
            ? stockIssueRepository.GetAllWithAuditIncludingDeleted().Where(i => i.Entity.IsDeleted)
            : stockIssueRepository.GetAllWithAudit();

        if (request.IssueType.HasValue)
        {
            source = source.Where(i => i.Entity.IssueType == request.IssueType.Value);
        }

        return Task.FromResult(source.MapTo().AsQueryable());
    }
}
