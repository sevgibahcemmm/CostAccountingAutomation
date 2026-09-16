using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ProductMovements;

[Permission("stock_movement:delete")]
public sealed record ProductMovementDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class ProductMovementDeleteCommandHandler(
    IProductMovementRepository productMovementRepository,
    IChartOfAccountLedgerRepository ledgerRepository) : IRequestHandler<ProductMovementDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductMovementDeleteCommand request, CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);
        ProductMovement? movement = await productMovementRepository.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (movement is null)
        {
            return Result<string>.Failure("Stok hareketi bulunamadı.");
        }

        List<ChartOfAccountLedger> ledgerEntries = await ledgerRepository
            .GetBySourceAsync("StokGirisi", request.Id, cancellationToken);

        ledgerEntries.AddRange(await ledgerRepository
            .GetBySourceAsync("StokCikisi", request.Id, cancellationToken));

        if (ledgerEntries.Count > 0)
        {
            ledgerRepository.SoftDeleteRange(ledgerEntries);
        }

        productMovementRepository.SoftDelete(movement);
        return Result<string>.Succeed("Stok hareketi başarıyla silindi.");
    }
}
