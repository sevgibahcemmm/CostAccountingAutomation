using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
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
    IStockIssueRepository stockIssueRepository,
    IAuditUserNameResolver auditUserNames) : IRequestHandler<StockIssueGetAllQuery, IQueryable<StockIssueListDto>>
{
    public async Task<IQueryable<StockIssueListDto>> Handle(StockIssueGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<StockIssue> source = request.OnlyDeleted
            ? stockIssueRepository.GetAllIncludingDeleted().Where(i => i.IsDeleted)
            : stockIssueRepository.GetAllActive();

        if (request.IssueType is StockIssueType issueType)
        {
            source = source.Where(i => i.IssueType == issueType);
        }

        return await source
            .MapTo()
            .ToAuditedListAsync(auditUserNames, cancellationToken);
    }
}
