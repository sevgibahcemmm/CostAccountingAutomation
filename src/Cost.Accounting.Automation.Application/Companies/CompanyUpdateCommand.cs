using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.Companies.ValueObjects;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Companies;

[Permission("company:update")]
public sealed record CompanyUpdateCommand(
    Guid Id,
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

public sealed class CompanyUpdateCommandValidator : AbstractValidator<CompanyUpdateCommand>
{
    public CompanyUpdateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Geçerli bir ID girin");

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

internal sealed class CompanyUpdateCommandHandler(
    ICompanyRepository companyRepository) : IRequestHandler<CompanyUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CompanyUpdateCommand request, CancellationToken cancellationToken)
    {
        // Guid tipindeki Id'yi doğrudan IdentityId'ye veriyoruz (new Guid hatasına düşmemek için)
        IdentityId companyId = new(request.Id);

        // Şirketi bul (Tracking ile çekiyoruz ki Update tetiklensin)
        var company = await companyRepository.GetByExpressionWithTrackingAsync(
            p => p.Id == companyId,
            cancellationToken);

        if (company is null)
        {
            return Result<string>.Failure("Şirket bulunamadı");
        }

        // Aynı isimde başka bir şirket var mı kontrol et (kendisi hariç)
        var nameExists = await companyRepository.AnyAsync(
            p => p.Name.Value == request.Name && p.Id != companyId,
            cancellationToken);

        if (nameExists)
        {
            return Result<string>.Failure("Bu şirket adı başka bir şirket tarafından kullanılıyor");
        }

        // Aynı vergi numarasında başka bir şirket var mı kontrol et (kendisi hariç)
        var taxNumberExists = await companyRepository.AnyAsync(
            p => p.TaxNumber.Value == request.TaxNumber && p.Id != companyId,
            cancellationToken);

        if (taxNumberExists)
        {
            return Result<string>.Failure("Bu vergi numarası başka bir şirket tarafından kullanılıyor");
        }

        // --- Domain Setter Metotları ile Güncelleme ---
        company.SetName(new Name(request.Name));
        company.SetTaxOffice(new TaxOffice(request.TaxOffice));
        company.SetTaxNumber(new TaxNumber(request.TaxNumber));
        company.SetDescription(new Description(request.Description));
        company.SetInvoiceinformation(new Invoiceinformation(request.Invoiceinformation));
        company.SetLetterhead(new Letterhead(request.Letterhead));
        company.SetCompanyPrefix(new CompanyPrefix(request.CompanyPrefix));
        company.SetAddress(request.Address);
        company.SetContact(request.Contact);
        company.SetStatus(request.IsActive);
        company.SetExpenditureUnit(new ExpenditureUnit(request.ExpenditureUnitName, request.ExpenditureUnitCode));
        company.SetAccountingUnit(new AccountingUnit(request.AccountingUnitName, request.AccountingUnitCode));

        companyRepository.Update(company);

        return "Şirket başarıyla güncellendi";
    }
}