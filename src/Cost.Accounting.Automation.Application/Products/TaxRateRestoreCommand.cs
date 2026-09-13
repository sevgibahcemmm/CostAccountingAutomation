using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products;

[Permission("product:delete")]
public sealed record TaxRateRestoreCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class TaxRateRestoreCommandHandler(
    ITaxRateRepository taxRateRepository) : IRequestHandler<TaxRateRestoreCommand, Result<string>>
{
    public async Task<Result<string>> Handle(TaxRateRestoreCommand request, CancellationToken cancellationToken)
    {
        var taxRate = await taxRateRepository.GetByIdIncludingDeletedAsync(new IdentityId(request.Id), cancellationToken);
        if (taxRate is null)
        {
            return Result<string>.Failure("KDV oranı bulunamadı");
        }

        if (!taxRate.IsDeleted)
        {
            return Result<string>.Failure("KDV oranı zaten silinmiş durumda değil");
        }

        taxRate.Restore();
        taxRateRepository.Update(taxRate);

        return "KDV oranı başarıyla geri yüklendi";
    }
}