using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

[Permission("chartofaccount:view")]
public sealed record ChartOfAccountLedgerSyncCommand() : IRequest<Result<string>>;

internal sealed class ChartOfAccountLedgerSyncCommandHandler(
    IProductMovementRepository productMovementRepository,
    IChartOfAccountLedgerRepository ledgerRepository) : IRequestHandler<ChartOfAccountLedgerSyncCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ChartOfAccountLedgerSyncCommand request, CancellationToken cancellationToken)
    {
        List<ProductMovement> movements = await productMovementRepository
            .GetAll()
            .Where(m => !m.IsDeleted && m.Product != null && m.Product.ChartOfAccountId != null)
            .Include(m => m.Product)
            .ToListAsync(cancellationToken);

        HashSet<string> existingKeys = await ledgerRepository.GetExistingSourceKeysAsync(cancellationToken);

        List<ChartOfAccountLedger> entries = [];

        foreach (ProductMovement movement in movements)
        {
            if (movement.UnitPrice is not { } price || price.Value <= 0)
            {
                continue;
            }

            string sourceType = movement.InvoiceId is null
                ? movement.MovementType == ProductMovementType.Input ? "StokGirisi" : "StokCikisi"
                : movement.MovementType == ProductMovementType.Input ? "SatinalmaFaturasi" : "SatisFaturasi";

            string key = $"{sourceType}|{movement.Id.Value}";
            if (existingKeys.Contains(key))
            {
                continue;
            }

            decimal amount = Math.Round(movement.Quantity * price.Value, 2);
            bool isDebit = movement.MovementType == ProductMovementType.Input;

            entries.Add(new ChartOfAccountLedger(
                movement.Product!.ChartOfAccountId!,
                isDebit ? amount : 0,
                isDebit ? 0 : amount,
                sourceType,
                movement.Id));
        }

        if (entries.Count == 0)
        {
            return Result<string>.Succeed("Yevmiye kayıtları güncel. Eklenecek kayıt yok.");
        }

        await ledgerRepository.AddRangeAsync(entries, cancellationToken);

        return Result<string>.Succeed($"{entries.Count} stok hareketi yevmiyeye aktarıldı.");
    }
}