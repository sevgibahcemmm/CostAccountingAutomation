using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

[Permission("chartofaccount:update")]
public sealed record ConsumptionUnitUpdateCommand(
    Guid Id,
    string Name,
    bool IsActive) : IRequest<Result<string>>;

public sealed class ConsumptionUnitUpdateCommandValidator : AbstractValidator<ConsumptionUnitUpdateCommand>
{
    public ConsumptionUnitUpdateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Geçerli bir kayıt seçilmelidir.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tüketim birimi adı zorunludur.")
            .MaximumLength(120).WithMessage("Tüketim birimi adı en fazla 120 karakter olabilir.");
    }
}

internal sealed class ConsumptionUnitUpdateCommandHandler(
    IChartOfAccountRepository chartOfAccountRepository) : IRequestHandler<ConsumptionUnitUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ConsumptionUnitUpdateCommand request, CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);

        ChartOfAccount? unit = await chartOfAccountRepository.GetByExpressionWithTrackingAsync(
            x => x.Id == id,
            cancellationToken);

        if (unit is null || unit.Type != ChartOfAccountType.ConsumptionUnit)
        {
            return Result<string>.Failure("Tüketim birimi bulunamadı.");
        }

        string name = request.Name.Trim();

        bool nameExists = await chartOfAccountRepository.AnyAsync(
            x => x.Id != id &&
                 x.Type == ChartOfAccountType.ConsumptionUnit &&
                 x.Name.Value == name,
            cancellationToken);

        if (nameExists)
        {
            return Result<string>.Failure("Bu tüketim birimi adı başka bir kayıt tarafından kullanılıyor.");
        }

        unit.SetName(new Name(name));
        unit.SetStatus(request.IsActive);

        chartOfAccountRepository.Update(unit);

        return "Tüketim birimi başarıyla güncellendi.";
    }
}
