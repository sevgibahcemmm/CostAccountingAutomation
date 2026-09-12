using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies.ValueObjects;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.Suppliers;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Suppliers;

[Permission("supplier:update")]
public sealed record SupplierUpdateCommand(
    Guid Id,
    string Name,
    string TaxOffice,
    string TaxNumber,
    Address Address,
    Contact Contact,
    string Description,
    bool IsActive) : IRequest<Result<string>>;

public sealed class SupplierUpdateCommandValidator : AbstractValidator<SupplierUpdateCommand>
{
    public SupplierUpdateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Geçerli bir ID girin");

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

internal sealed class SupplierUpdateCommandHandler(
    ISupplierRepository supplierRepository) : IRequestHandler<SupplierUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(SupplierUpdateCommand request, CancellationToken cancellationToken)
    {
        IdentityId supplierId = new(request.Id);

        var supplier = await supplierRepository.GetByExpressionWithTrackingAsync(
            p => p.Id == supplierId,
            cancellationToken);

        if (supplier is null)
        {
            return Result<string>.Failure("Tedarikçi bulunamadı");
        }

        var nameExists = await supplierRepository.AnyAsync(
            p => p.Name.Value == request.Name && p.Id != supplierId,
            cancellationToken);

        if (nameExists)
        {
            return Result<string>.Failure("Bu tedarikçi adı başka bir tedarikçi tarafından kullanılıyor");
        }

        supplier.SetName(new Name(request.Name));
        supplier.SetTaxOffice(new TaxOffice(request.TaxOffice));
        supplier.SetTaxNumber(new TaxNumber(request.TaxNumber));
        supplier.SetContact(request.Contact);
        supplier.SetAddress(request.Address);
        supplier.SetDescription(new Description(request.Description));
        supplier.SetStatus(request.IsActive);

        supplierRepository.Update(supplier);

        return "Tedarikçi başarıyla güncellendi";
    }
}