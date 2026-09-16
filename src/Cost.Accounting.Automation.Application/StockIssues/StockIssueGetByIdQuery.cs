using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.StockIssues;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.StockIssues;

[Permission("stock_issue:view")]
public sealed record StockIssueGetByIdQuery(Guid Id) : IRequest<Result<StockIssueDto>>;

internal sealed class StockIssueGetByIdQueryHandler(
    IStockIssueRepository stockIssueRepository) : IRequestHandler<StockIssueGetByIdQuery, Result<StockIssueDto>>
{
    public async Task<Result<StockIssueDto>> Handle(StockIssueGetByIdQuery request, CancellationToken cancellationToken)
    {
        StockIssue? issue = await stockIssueRepository.GetWithDetailsAsync(new IdentityId(request.Id), cancellationToken);

        if (issue is null)
        {
            return Result<StockIssueDto>.Failure("Belge bulunamadı.");
        }

        return Result<StockIssueDto>.Succeed(issue.ToDto());
    }
}
