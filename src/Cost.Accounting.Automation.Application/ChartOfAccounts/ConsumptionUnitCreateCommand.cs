using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

[Permission("chartofaccount:create")]
public sealed record ConsumptionUnitCreateCommand(
    string Name,
    string? Code,
    bool IsActive) : IRequest<Result<string>>;

public sealed class ConsumptionUnitCreateCommandValidator : AbstractValidator<ConsumptionUnitCreateCommand>
{
    public ConsumptionUnitCreateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tüketim birimi adı zorunludur.")
            .MaximumLength(120).WithMessage("Tüketim birimi adı en fazla 120 karakter olabilir.");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Kod en fazla 50 karakter olabilir.");
    }
}

internal sealed class ConsumptionUnitCreateCommandHandler(
    IChartOfAccountRepository chartOfAccountRepository,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<ConsumptionUnitCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ConsumptionUnitCreateCommand request, CancellationToken cancellationToken)
    {
        string name = request.Name.Trim();

        List<ChartOfAccount> all = await chartOfAccountRepository.GetAllIncludingDeletedAsync(cancellationToken);

        ChartOfAccount? root = ConsumptionUnitHelper.FindRoot(all);

        if (root is null)
        {
            root = new ChartOfAccount(
                new AccountCode(ConsumptionUnitHelper.RootCode),
                new Name(ConsumptionUnitHelper.RootName),
                level: 0,
                ChartOfAccountType.MainGroup);

            root.SetStatus(true);
            await chartOfAccountRepository.AddAsync(root, cancellationToken);
            all.Add(root);
        }

        string code = string.IsNullOrWhiteSpace(request.Code)
            ? ConsumptionUnitHelper.BuildNextCode(all)
            : ConsumptionUnitHelper.NormalizeCode(request.Code);

        if (string.IsNullOrEmpty(code))
        {
            return Result<string>.Failure("Geçerli bir kod üretilemedi.");
        }

        string? codeKey = ChartOfAccount.BuildDuplicateKey(code);

        ChartOfAccount? codeDuplicate = await duplicateCheckService.FindDuplicateAsync<ChartOfAccount>(
            codeKey,
            includeDeleted: true,
            cancellationToken: cancellationToken);

        if (codeDuplicate is not null)
        {
            return Result<string>.Failure($"'{code}' kodu zaten kullanılıyor.");
        }

        bool nameExists = all.Any(a =>
            !a.IsDeleted &&
            a.Type == ChartOfAccountType.ConsumptionUnit &&
            string.Equals(a.Name.Value, name, StringComparison.OrdinalIgnoreCase));

        if (nameExists)
        {
            return Result<string>.Failure("Bu tüketim birimi adı zaten kullanılıyor.");
        }

        ChartOfAccount unit = new(
            new AccountCode(code),
            new Name(name),
            level: root.Level + 1,
            ChartOfAccountType.ConsumptionUnit);

        unit.SetParent(root.Id);
        unit.SetStatus(request.IsActive);

        await chartOfAccountRepository.AddAsync(unit, cancellationToken);

        return $"'{code} - {name}' tüketim birimi başarıyla kaydedildi.";
    }
}
