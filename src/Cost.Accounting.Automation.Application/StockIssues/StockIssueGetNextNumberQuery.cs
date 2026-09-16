using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.StockIssues;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.StockIssues;

[Permission("stock_issue:view")]
public sealed record StockIssueGetNextNumberQuery(StockIssueType IssueType) : IRequest<Result<string>>;

internal sealed class StockIssueGetNextNumberQueryHandler(
    IStockIssueRepository stockIssueRepository) : IRequestHandler<StockIssueGetNextNumberQuery, Result<string>>
{
    public async Task<Result<string>> Handle(StockIssueGetNextNumberQuery request, CancellationToken cancellationToken)
    {
        int count = await stockIssueRepository
            .GetAllWithAuditIncludingDeleted()
            .CountAsync(i => i.Entity.IssueType == request.IssueType, cancellationToken);

        string prefix = request.IssueType == StockIssueType.Consumption ? "TUK" : "ATL";

        return Result<string>.Succeed($"{prefix}-{DateTime.Now:yyyy}-{count + 1:00000}");
    }
}
