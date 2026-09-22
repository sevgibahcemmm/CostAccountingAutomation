using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Products.TaxRates;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products.TaxRates;

[Permission("product:create")]
public sealed record TaxRateCreateCommand(
    string Name,
    decimal Rate,
    bool IsActive) : IRequest<Result<string>>;

public sealed class TaxRateCreateCommandValidator : AbstractValidator<TaxRateCreateCommand>
{
    public TaxRateCreateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Geçerli bir KDV adı girin")
            .MaximumLength(120).WithMessage("KDV adı en fazla 120 karakter olabilir");

        RuleFor(x => x.Rate)
            .InclusiveBetween(0m, 1m).WithMessage("KDV oranı %0 ile %100 arasında olmalıdır");
    }
}

internal sealed class TaxRateCreateCommandHandler(
    ITaxRateRepository taxRateRepository,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<TaxRateCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(TaxRateCreateCommand request, CancellationToken cancellationToken)
    {
        string? duplicateKey = TaxRate.BuildDuplicateKey(request.Name);

        TaxRate? nameDuplicate = await duplicateCheckService.FindDuplicateAsync<TaxRate>(
            duplicateKey,
            cancellationToken: cancellationToken);

        if (nameDuplicate is not null)
        {
            return Result<string>.Failure("Bu KDV oranı daha önce kullanılmış");
        }

        TaxRate taxRate = new(
            new Name(request.Name),
            request.Rate,
            request.IsActive);

        await taxRateRepository.AddAsync(taxRate, cancellationToken);

        return $"KDV oranı (%{request.Rate * 100:0.###}) başarıyla kaydedildi";
    }
}