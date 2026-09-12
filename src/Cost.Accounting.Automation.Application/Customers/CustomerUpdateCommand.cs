using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies.ValueObjects;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Customers;

[Permission("customer:update")]
public sealed record CustomerUpdateCommand(
    Guid Id,
    string Name,
    string TaxOffice,
    string TaxNumber,
    Address Address,
    Contact Contact,
    string Description,
    bool IsActive) : IRequest<Result<string>>;

public sealed class CustomerUpdateCommandValidator : AbstractValidator<CustomerUpdateCommand>
{
    public CustomerUpdateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Geçerli bir ID girin");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Geçerli bir müşteri adı girin")
            .MaximumLength(200).WithMessage("Müşteri adı en fazla 200 karakter olabilir");

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

internal sealed class CustomerUpdateCommandHandler(
    ICustomerRepository customerRepository) : IRequestHandler<CustomerUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CustomerUpdateCommand request, CancellationToken cancellationToken)
    {
        IdentityId customerId = new(request.Id);

        var customer = await customerRepository.GetByExpressionWithTrackingAsync(
            p => p.Id == customerId,
            cancellationToken);

        if (customer is null)
        {
            return Result<string>.Failure("Müşteri bulunamadı");
        }

        var nameExists = await customerRepository.AnyAsync(
            p => p.Name.Value == request.Name && p.Id != customerId,
            cancellationToken);

        if (nameExists)
        {
            return Result<string>.Failure("Bu müşteri adı başka bir müşteri tarafından kullanılıyor");
        }

        customer.SetName(new Name(request.Name));
        customer.SetTaxOffice(new TaxOffice(request.TaxOffice));
        customer.SetTaxNumber(new TaxNumber(request.TaxNumber));
        customer.SetContact(request.Contact);
        customer.SetAddress(request.Address);
        customer.SetDescription(new Description(request.Description));
        customer.SetStatus(request.IsActive);

        customerRepository.Update(customer);

        return "Müşteri başarıyla güncellendi";
    }
}