using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

[Permission("chartofaccount:create")]
public sealed record ChartOfAccountManualCreateCommand(
    Guid? ParentId,
    string? Code,
    string Name,
    bool IsActive) : IRequest<Result<string>>;

public sealed class ChartOfAccountManualCreateCommandValidator : AbstractValidator<ChartOfAccountManualCreateCommand>
{
    public ChartOfAccountManualCreateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Hesap adı zorunludur.")
            .MaximumLength(120).WithMessage("Hesap adı en fazla 120 karakter olabilir.");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Kod en fazla 50 karakter olabilir.");
    }
}

internal sealed class ChartOfAccountManualCreateCommandHandler(
    IChartOfAccountRepository chartOfAccountRepository) : IRequestHandler<ChartOfAccountManualCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ChartOfAccountManualCreateCommand request, CancellationToken cancellationToken)
    {
        string name = request.Name.Trim();

        List<ChartOfAccount> all = await chartOfAccountRepository.GetAllIncludingDeletedAsync(cancellationToken);

        ChartOfAccount? parent = null;
        string? parentCode = null;

        if (request.ParentId is Guid parentId)
        {
            parent = all.FirstOrDefault(a => a.Id == parentId);

            if (parent is null)
            {
                return Result<string>.Failure("Üst hesap bulunamadı.");
            }

            parentCode = ChartOfAccountCodeHelper.NormalizeCode(parent.Code.Value);
        }

        string code = string.IsNullOrWhiteSpace(request.Code)
            ? parentCode is null
                ? string.Empty
                : ChartOfAccountCodeHelper.BuildNextChildCode(all, parentCode)
            : ChartOfAccountCodeHelper.NormalizeCode(request.Code);

        if (string.IsNullOrEmpty(code))
        {
            return Result<string>.Failure("Geçerli bir kod üretilemedi. Üst hesap seçin veya kod girin.");
        }

        if (parentCode is not null && !ChartOfAccountCodeHelper.IsChildOf(code, parentCode))
        {
            return Result<string>.Failure($"Alt kod, üst hesabın ({parentCode}) altı olmalıdır. Örnek: {parentCode}.01");
        }

        ChartOfAccount? duplicate = all.FirstOrDefault(a =>
            string.Equals(a.Code.Value, code, StringComparison.OrdinalIgnoreCase));

        if (duplicate is not null)
        {
            return Result<string>.Failure($"'{code}' kodu zaten kullanılıyor.");
        }

        ChartOfAccountType type = ChartOfAccountCodeHelper.DetermineLeafType(code);

        ChartOfAccount account = new(
            new AccountCode(code),
            new Name(name),
            level: parent is null ? 0 : parent.Level + 1,
            type);

        account.SetParent(parent?.Id);
        account.SetStatus(request.IsActive);

        await chartOfAccountRepository.AddAsync(account, cancellationToken);

        return $"'{code} - {name}' hesabı başarıyla kaydedildi.";
    }
}