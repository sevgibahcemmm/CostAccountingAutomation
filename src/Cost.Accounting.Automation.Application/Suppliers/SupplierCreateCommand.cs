using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Companies.ValueObjects;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.Suppliers;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Suppliers;

[Permission("supplier:create")]
public sealed record SupplierCreateCommand(
    string Name,
    string TaxOffice,
    string TaxNumber,
    Address Address,
    Contact Contact,
    string Description,
    bool IsActive) : IRequest<Result<string>>;

public sealed class SupplierCreateCommandValidator : AbstractValidator<SupplierCreateCommand>
{
    public SupplierCreateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Geçerli bir tedarikçi adı girin")
            .MaximumLength(200).WithMessage("Tedarikçi adı en fazla 200 karakter olabilir");

        RuleFor(x => x.Address.City)
            .NotEmpty().WithMessage("Geçerli bir şehir girin");

        RuleFor(x => x.Address.District)
            .NotEmpty().WithMessage("Geçerli bir ilçe girin");

        RuleFor(x => x.Address.FullAddress)
            .NotEmpty().WithMessage("Geçerli bir tam adres girin");

        RuleFor(x => x.Contact.PhoneNumber1)
            .NotEmpty().WithMessage("Geçerli bir telefon numarası girin");
    }
}

internal sealed class SupplierCreateCommandHandler(
    ISupplierRepository supplierRepository,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<SupplierCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(SupplierCreateCommand request, CancellationToken cancellationToken)
    {
        string? duplicateKey = Supplier.BuildDuplicateKey(request.Name);

        Supplier? nameDuplicate = await duplicateCheckService.FindDuplicateAsync<Supplier>(
            duplicateKey,
            cancellationToken: cancellationToken);

        if (nameDuplicate is not null)
        {
            return Result<string>.Failure("Bu tedarikçi adı daha önce kullanılmış");
        }

        Supplier supplier = new(
            new Name(request.Name),
            new TaxOffice(request.TaxOffice),
            new TaxNumber(request.TaxNumber),
            request.Contact,
            request.Address,
            new Description(request.Description),
            request.IsActive);

        await supplierRepository.AddAsync(supplier, cancellationToken);

        return "Tedarikçi başarıyla kaydedildi";
    }
}