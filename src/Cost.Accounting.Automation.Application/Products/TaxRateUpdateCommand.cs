using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products;

[Permission("product:update")]
public sealed record TaxRateUpdateCommand(
    Guid Id,
    string Name,
    decimal Rate,
    bool IsActive) : IRequest<Result<string>>;

public sealed class TaxRateUpdateCommandValidator : AbstractValidator<TaxRateUpdateCommand>
{
    public TaxRateUpdateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Geçerli bir ID girin");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Geçerli bir KDV adı girin")
            .MaximumLength(120).WithMessage("KDV adı en fazla 120 karakter olabilir");

        RuleFor(x => x.Rate)
            .InclusiveBetween(0m, 1m).WithMessage("KDV oranı %0 ile %100 arasında olmalıdır");
    }
}

internal sealed class TaxRateUpdateCommandHandler(
    ITaxRateRepository taxRateRepository) : IRequestHandler<TaxRateUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(TaxRateUpdateCommand request, CancellationToken cancellationToken)
    {
        IdentityId taxRateId = new(request.Id);

        var taxRate = await taxRateRepository.GetByExpressionWithTrackingAsync(
            p => p.Id == taxRateId,
            cancellationToken);

        if (taxRate is null)
        {
            return Result<string>.Failure("KDV oranı bulunamadı");
        }

        var nameExists = await taxRateRepository.AnyAsync(
            p => p.Name.Value == request.Name && p.Id != taxRateId,
            cancellationToken);

        if (nameExists)
        {
            return Result<string>.Failure("Bu KDV adı başka bir kayıt tarafından kullanılıyor");
        }

        taxRate.SetName(new Name(request.Name));
        taxRate.SetRate(request.Rate);
        taxRate.SetStatus(request.IsActive);

        taxRateRepository.Update(taxRate);

        return $"KDV oranı (%{request.Rate * 100:0.###}) başarıyla güncellendi";
    }
}