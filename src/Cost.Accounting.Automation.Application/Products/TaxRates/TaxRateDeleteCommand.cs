using Cost.Accounting.Automation.Application;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Products.TaxRates;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products.TaxRates;

[Permission("product:delete")]
public sealed record TaxRateDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class TaxRateDeleteCommandHandler(
    ITaxRateRepository taxRateRepository,
    IProductRepository productRepository) : IRequestHandler<TaxRateDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(TaxRateDeleteCommand request, CancellationToken cancellationToken)
    {
        var taxRate = await taxRateRepository.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (taxRate is null)
        {
            return Result<string>.Failure("KDV oranı bulunamadı");
        }

        bool usedByProduct = await productRepository.AnyAsync(p => p.TaxRateId == request.Id, cancellationToken);

        taxRate.Delete();
        taxRateRepository.Update(taxRate);

        if (usedByProduct)
        {
            return DeleteWarnings.Compose(
                $"'{taxRate.Name.Value}' KDV oranı silindi, ancak ilişkili ürün kayıtlarında kullanıldığı için " +
                $"ilgili ürünlerin gözden geçirilmesi gerekir.");
        }

        return "KDV oranı başarıyla silindi";
    }
}