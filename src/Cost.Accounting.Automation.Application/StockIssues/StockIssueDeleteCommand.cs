using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.StockIssues;

[Permission("stock_issue:delete")]
public sealed record StockIssueDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class StockIssueDeleteCommandHandler(
    IStockIssueRepository stockIssueRepository,
    IProductMovementRepository productMovementRepository,
    IChartOfAccountLedgerRepository ledgerRepository) : IRequestHandler<StockIssueDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(StockIssueDeleteCommand request, CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);

        StockIssue? issue = await stockIssueRepository.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (issue is null)
        {
            return Result<string>.Failure("Belge bulunamadı.");
        }

        List<ProductMovement> movements = await productMovementRepository
            .Where(m => m.StockIssueId == id)
            .ToListAsync(cancellationToken);

        List<ChartOfAccountLedger> ledgerEntries = [];

        foreach (ProductMovement movement in movements)
        {
            ledgerEntries.AddRange(await ledgerRepository.GetBySourceAsync("StokTuketimi", movement.Id.Value, cancellationToken));
            ledgerEntries.AddRange(await ledgerRepository.GetBySourceAsync("AtolyeTransferi", movement.Id.Value, cancellationToken));
        }

        if (ledgerEntries.Count > 0)
        {
            ledgerRepository.SoftDeleteRange(ledgerEntries);
        }

        foreach (ProductMovement movement in movements)
        {
            productMovementRepository.SoftDelete(movement);
        }

        stockIssueRepository.SoftDelete(issue);

        return Result<string>.Succeed("Belge ve ilişkili hareketler başarıyla silindi.");
    }
}
