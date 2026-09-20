using Cost.Accounting.Automation.Application;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products;

[Permission("product:delete")]
public sealed record ProductDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class ProductDeleteCommandHandler(
    IProductRepository productRepository,
    IChartOfAccountRepository chartOfAccountRepository,
    IProductMovementRepository productMovementRepository,
    IInvoiceRepository invoiceRepository,
    ICostSlipRepository costSlipRepository,
    IStockIssueRepository stockIssueRepository) : IRequestHandler<ProductDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductDeleteCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (product is null)
        {
            return Result<string>.Failure("Ürün bulunamadı");
        }

        bool hasMovement = await productMovementRepository.AnyAsync(
            m => m.ProductId == request.Id, cancellationToken);
        if (hasMovement)
        {
            return Result<string>.Failure(
                $"'{product.Name.Value}' ürünü işlem/hareket gördüğü için silinemez.");
        }

        bool hasInvoiceLine = await invoiceRepository.AnyAsync(
            i => i.Lines.Any(l => l.ProductId == request.Id), cancellationToken);
        bool hasCostSlipItem = await costSlipRepository.AnyAsync(
            c => c.ProducedProductId == new IdentityId(request.Id)
                 || c.CostSlipItems.Any(i => i.ProductId == new IdentityId(request.Id)),
            cancellationToken);
        bool hasStockIssueLine = await stockIssueRepository.AnyAsync(
            s => s.Lines.Any(l => l.ProductId == request.Id), cancellationToken);
        bool hasLinkedProduct = await productRepository.AnyAsync(
            p => p.SemiFinishedProductId == new IdentityId(request.Id), cancellationToken);

        if (product.ChartOfAccountId is { } nodeId)
        {
            List<ChartOfAccount> accounts = await chartOfAccountRepository.GetAllIncludingDeletedAsync(cancellationToken);
            ChartOfAccount? node = accounts.FirstOrDefault(a => a.Id == nodeId && !a.IsDeleted);
            if (node is not null)
            {
                node.Delete();
                chartOfAccountRepository.Update(node);
            }
        }

        product.Delete();
        productRepository.Update(product);

        if (hasInvoiceLine || hasCostSlipItem || hasStockIssueLine || hasLinkedProduct)
        {
            return DeleteWarnings.Compose(
                $"'{product.Name.Value}' ürünü silindi. NOT: irsaliye/maliyet pusulası/stok çıkışı kayıtlarında " +
                $"kullanılıyor; hareket görmediği için silme gerçekleştirildi.");
        }

        return "Ürün başarıyla silindi";
    }
}