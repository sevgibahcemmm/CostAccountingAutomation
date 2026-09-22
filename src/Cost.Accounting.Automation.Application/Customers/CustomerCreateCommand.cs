using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Companies.ValueObjects;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Customers;

[Permission("customer:create")]
public sealed record CustomerCreateCommand(
    string Name,
    string TaxOffice,
    string TaxNumber,
    Address Address,
    Contact Contact,
    string Description,
    bool IsActive) : IRequest<Result<string>>;

public sealed class CustomerCreateCommandValidator : AbstractValidator<CustomerCreateCommand>
{
    public CustomerCreateCommandValidator()
    {
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

internal sealed class CustomerCreateCommandHandler(
    ICustomerRepository customerRepository,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<CustomerCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CustomerCreateCommand request, CancellationToken cancellationToken)
    {
        string? duplicateKey = Customer.BuildDuplicateKey(request.Name);

        Customer? nameDuplicate = await duplicateCheckService.FindDuplicateAsync<Customer>(
            duplicateKey,
            cancellationToken: cancellationToken);

        if (nameDuplicate is not null)
        {
            return Result<string>.Failure("Bu müşteri adı daha önce kullanılmış");
        }

        Customer customer = new(
            new Name(request.Name),
            new TaxOffice(request.TaxOffice),
            new TaxNumber(request.TaxNumber),
            request.Contact,
            request.Address,
            new Description(request.Description),
            request.IsActive);

        await customerRepository.AddAsync(customer, cancellationToken);

        return "Müşteri başarıyla kaydedildi";
    }
}