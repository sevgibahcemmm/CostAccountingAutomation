using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.Companies.ValueObjects;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Companies;

[Permission("company:create")]
public sealed record CompanyCreateCommand(
    string Name,
    string TaxOffice,
    string TaxNumber,
    string Description,
    string Invoiceinformation,
    string Letterhead,
    string CompanyPrefix,
    Address Address,
    Contact Contact,
    string ExpenditureUnitName,
    string ExpenditureUnitCode,
    string AccountingUnitName,
    string AccountingUnitCode,
    bool IsActive) : IRequest<Result<string>>;

public sealed class CompanyCreateCommandValidator : AbstractValidator<CompanyCreateCommand>
{
    public CompanyCreateCommandValidator()
    {

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Geçerli bir Şirket adı girin")
            .MaximumLength(200).WithMessage("Şirket adı en fazla 200 karakter olabilir");

        RuleFor(x => x.TaxOffice)
            .NotEmpty().WithMessage("Geçerli bir Vergi Dairesi girin");

        RuleFor(x => x.TaxNumber)
            .NotEmpty().WithMessage("Geçerli bir Vergi Numarası girin")
            .Matches(@"^\d{10}$|^\d{11}$").WithMessage("Vergi numarası 10 veya 11 haneli olmalıdır");

        RuleFor(x => x.Invoiceinformation)
            .NotEmpty().WithMessage("Fatura bilgisi giriniz");

        RuleFor(x => x.CompanyPrefix)
            .NotEmpty().WithMessage("Şirket ön eki giriniz")
            .MaximumLength(10).WithMessage("Şirket ön eki en fazla 10 karakter olabilir");

        RuleFor(x => x.ExpenditureUnitName)
            .NotEmpty().WithMessage("Harcama birimi adı boş olamaz");

        RuleFor(x => x.AccountingUnitName)
            .NotEmpty().WithMessage("Muhasebe birimi adı boş olamaz");

        RuleFor(x => x.Address.City)
            .NotEmpty().WithMessage("Geçerli bir şehir seçin");

        RuleFor(x => x.Address.District)
            .NotEmpty().WithMessage("Geçerli bir ilçe seçin");

        RuleFor(x => x.Address.FullAddress)
            .NotEmpty().WithMessage("Geçerli bir tam adres girin");

        RuleFor(x => x.Contact.PhoneNumber1)
            .NotEmpty().WithMessage("Geçerli bir telefon numarası girin");
    }
}

internal sealed class CompanyCreateCommandHandler(
    ICompanyRepository companyRepository,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<CompanyCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CompanyCreateCommand request, CancellationToken cancellationToken)
    {
        // Şirket adı kontrolü
        string? duplicateKey = Company.BuildDuplicateKey(request.Name);

        Company? nameDuplicate = await duplicateCheckService.FindMasterDuplicateAsync<Company>(
            duplicateKey,
            cancellationToken: cancellationToken);

        if (nameDuplicate is not null)
        {
            return Result<string>.Failure("Bu şirket adı daha önce kullanılmış");
        }

        // Vergi numarası kontrolü
        var taxNumberExists = await companyRepository.AnyAsync(
            p => p.TaxNumber.Value == request.TaxNumber,
            cancellationToken);

        if (taxNumberExists)
        {
            return Result<string>.Failure("Bu vergi numarası daha önce kullanılmış");
        }

        // --- Value Object'leri ve Entity'yi Doğru Tiplerle Oluştur ---

        // Şirket temel bilgileri
        Name name = new(request.Name);
        TaxOffice taxOffice = new(request.TaxOffice);
        TaxNumber taxNumber = new(request.TaxNumber);
        Description description = new(request.Description);
        Invoiceinformation invoiceinformation = new(request.Invoiceinformation);
        Letterhead letterhead = new(request.Letterhead);
        CompanyPrefix companyPrefix = new(request.CompanyPrefix);

        // Raporlama için yeni eklediğimiz Value Object'ler (Kritik Düzeltme Burası)
        ExpenditureUnit expenditureUnit = new(request.ExpenditureUnitName, request.ExpenditureUnitCode);
        AccountingUnit accountingUnit = new(request.AccountingUnitName, request.AccountingUnitCode);

        // Company entity oluştur (Constructor sırasına ve tiplerine sadık kalınarak)
        Company company = new(
            name,
            taxOffice,
            taxNumber,
            description,
            invoiceinformation,
            letterhead,
            companyPrefix,
            request.Address,
            request.Contact,
            expenditureUnit,
            accountingUnit,
            request.IsActive
        );

        await companyRepository.AddAsync(company, cancellationToken);

        return "Şirket başarıyla kaydedildi";
    }
}