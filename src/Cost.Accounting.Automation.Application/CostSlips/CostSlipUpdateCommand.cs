using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips.CostSlipItems;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.CostSlips;

[Permission("costslip:update")]
public sealed record CostSlipUpdateCommand(
    Guid Id,
    string SlipNumber,
    CostSlipType CostSlipType,
    DateOnly CostDate,
    Guid WorkshopId,
    Guid? ProducedProductId,
    Guid? CustomerId,
    int Quantity,
    string? Description,
    List<CostSlipItemModel> Items) : IRequest<Result<string>>;

public sealed class CostSlipUpdateCommandValidator : AbstractValidator<CostSlipUpdateCommand>
{
    public CostSlipUpdateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Geçerli bir pusula ID girin.");

        RuleFor(x => x.SlipNumber)
            .NotEmpty().WithMessage("Pusula numarası boş olamaz.")
            .MaximumLength(100).WithMessage("Pusula numarası en fazla 100 karakter olabilir.");

        RuleFor(x => x.WorkshopId)
            .NotEmpty().WithMessage("Atölye seçilmelidir.");

        RuleFor(x => x.CostDate)
            .NotEqual(default(DateOnly)).WithMessage("Tarih seçilmelidir.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Üretim miktarı sıfırdan büyük olmalıdır.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Pusulada en az bir gider kalemi bulunmalıdır.");

        When(x => x.CostSlipType == CostSlipType.Product || x.CostSlipType == CostSlipType.SemiFinishedProduct, () =>
        {
            RuleFor(x => x.ProducedProductId)
                .NotEmpty().WithMessage("Mamul / yarı mamul pusulası için üretilen ürün seçilmelidir.");
        });

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ExpenseAccountType)
                .IsInEnum().WithMessage("Geçerli bir gider hesabı seçilmelidir.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Kalem miktarı sıfırdan büyük olmalıdır.");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Kalem birim fiyatı negatif olamaz.");
        });

        RuleFor(x => x.Items.Sum(i => i.Quantity * i.UnitPrice))
            .GreaterThan(0).WithMessage("Pusula genel toplamı sıfırdan büyük olmalıdır.");
    }
}

internal sealed class CostSlipUpdateCommandHandler(
    ICostSlipRepository costSlipRepository,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<CostSlipUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CostSlipUpdateCommand request, CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);

        CostSlip? slip = await costSlipRepository
            .WhereWithTracking(s => s.Id == id)
            .Include(s => s.CostSlipItems)
            .FirstOrDefaultAsync(cancellationToken);

        if (slip is null)
        {
            return Result<string>.Failure("Maliyet pusulası bulunamadı.");
        }

        if (slip.Status == CostSlipStatus.Approved)
        {
            return Result<string>.Failure("Onaylanmış maliyet pusulaları düzenlenemez.");
        }

        string? duplicateKey = CostSlip.BuildDuplicateKey(request.SlipNumber, request.CostSlipType);

        CostSlip? numberDuplicate = await duplicateCheckService.FindDuplicateAsync<CostSlip>(
            duplicateKey,
            excludeId: request.Id,
            includeDeleted: true,
            cancellationToken: cancellationToken);

        if (numberDuplicate is not null)
        {
            return Result<string>.Failure("Bu pusula numarası başka bir maliyet pusulası tarafından kullanılıyor.");
        }

        slip.SetSlipNumber(request.SlipNumber.Trim());
        slip.SetCostSlipType(request.CostSlipType);
        slip.SetCostDate(request.CostDate);
        slip.SetWorkshop(new IdentityId(request.WorkshopId));
        slip.SetProducedProduct(request.ProducedProductId.HasValue ? new IdentityId(request.ProducedProductId.Value) : null);
        slip.SetCustomer(request.CustomerId.HasValue ? new IdentityId(request.CustomerId.Value) : null);
        slip.SetQuantity(request.Quantity);
        slip.SetDescription(new Description(request.Description?.Trim() ?? string.Empty));

        List<CostSlipItem> lines = request.Items
            .Select(item => new CostSlipItem(
                slip.Id,
                item.ProductId.HasValue ? new IdentityId(item.ProductId.Value) : null,
                item.ProductUnitTypeId.HasValue ? new IdentityId(item.ProductUnitTypeId.Value) : null,
                item.ExpenseAccountType,
                item.Quantity,
                item.UnitPrice,
                item.Description?.Trim()))
            .ToList();

        slip.ReplaceItems(lines);
        costSlipRepository.Update(slip);

        return Result<string>.Succeed("Maliyet pusulası taslağı başarıyla güncellendi.");
    }
}