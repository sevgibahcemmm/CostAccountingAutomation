using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips.CostSlipItems;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.CostSlips;

public sealed record CostSlipItemModel(
    Guid? ProductId,
    Guid? ProductUnitTypeId,
    ExpenseAccountType ExpenseAccountType,
    decimal Quantity,
    decimal UnitPrice,
    string? Description);

public sealed record CostSlipCreateResult(
    Guid SlipId,
    string Message) : IResultMessage;

[Permission("costslip:create")]
public sealed record CostSlipCreateCommand(
    string SlipNumber,
    CostSlipType CostSlipType,
    DateOnly CostDate,
    Guid WorkshopId,
    Guid? ProducedProductId,
    Guid? CustomerId,
    int Quantity,
    string? Description,
    List<CostSlipItemModel> Items,
    bool IsApproved = false,
    StockCostingMethod CostingMethod = StockCostingMethod.Fifo) : IRequest<Result<CostSlipCreateResult>>;

public sealed class CostSlipCreateCommandValidator : AbstractValidator<CostSlipCreateCommand>
{
    public CostSlipCreateCommandValidator()
    {
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

        When(x => x.CostSlipType == CostSlipType.Service, () =>
        {
            RuleFor(x => x.ProducedProductId)
                .Empty().WithMessage("Hizmet pusulasında üretilen ürün seçilmemelidir.");
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

internal sealed class CostSlipCreateCommandHandler(
    ICostSlipRepository costSlipRepository,
    IChartOfAccountRepository chartOfAccountRepository,
    IProductRepository productRepository,
    IProductMovementRepository productMovementRepository,
    IChartOfAccountLedgerPoster ledgerPoster,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<CostSlipCreateCommand, Result<CostSlipCreateResult>>
{
    public async Task<Result<CostSlipCreateResult>> Handle(CostSlipCreateCommand request, CancellationToken cancellationToken)
    {
        string? duplicateKey = CostSlip.BuildDuplicateKey(request.SlipNumber, request.CostSlipType);

        CostSlip? duplicate = await duplicateCheckService.FindDuplicateAsync<CostSlip>(
            duplicateKey,
            includeDeleted: true,
            cancellationToken: cancellationToken);

        if (duplicate is not null && !duplicate.IsDeleted)
        {
            return Result<CostSlipCreateResult>.Failure("Bu pusula numarası ile kaydedilmiş bir maliyet pusulası zaten mevcut.");
        }

        ChartOfAccount? workshop = await chartOfAccountRepository.GetAll()
            .FirstOrDefaultAsync(a => a.Id == new IdentityId(request.WorkshopId), cancellationToken);

        if (workshop is null || workshop.IsDeleted || !workshop.IsActive || workshop.Type != ChartOfAccountType.Workshop)
        {
            return Result<CostSlipCreateResult>.Failure("Seçilen atölye bulunamadı veya geçerli bir atölye değil.");
        }

        Product? producedProduct = null;
        if (request.ProducedProductId.HasValue)
        {
            producedProduct = await productRepository.GetAll()
                .FirstOrDefaultAsync(p => p.Id == new IdentityId(request.ProducedProductId.Value), cancellationToken);

            if (producedProduct is null || !producedProduct.IsActive)
            {
                return Result<CostSlipCreateResult>.Failure("Üretilen ürün bulunamadı veya aktif değil.");
            }
        }

        bool isRestored = duplicate is not null;

        CostSlip slip;
        if (duplicate is not null)
        {
            duplicate.Restore();
            duplicate.ClearItems();
            duplicate.SetCostDate(request.CostDate);
            duplicate.SetWorkshop(new IdentityId(request.WorkshopId));
            duplicate.SetProducedProduct(request.ProducedProductId.HasValue ? new IdentityId(request.ProducedProductId.Value) : null);
            duplicate.SetCustomer(request.CustomerId.HasValue ? new IdentityId(request.CustomerId.Value) : null);
            duplicate.SetQuantity(request.Quantity);
            duplicate.SetDescription(new Description(request.Description?.Trim() ?? string.Empty));
            slip = duplicate;
        }
        else
        {
            slip = new CostSlip(
                request.SlipNumber.Trim(),
                request.CostSlipType,
                request.CostDate,
                new IdentityId(request.WorkshopId),
                request.ProducedProductId.HasValue ? new IdentityId(request.ProducedProductId.Value) : null,
                request.CustomerId.HasValue ? new IdentityId(request.CustomerId.Value) : null,
                request.Quantity,
                new Description(request.Description?.Trim() ?? string.Empty));
        }

        foreach (CostSlipItemModel item in request.Items)
        {
            slip.AddItem(new CostSlipItem(
                slip.Id,
                item.ProductId.HasValue ? new IdentityId(item.ProductId.Value) : null,
                item.ProductUnitTypeId.HasValue ? new IdentityId(item.ProductUnitTypeId.Value) : null,
                item.ExpenseAccountType,
                item.Quantity,
                item.UnitPrice,
                item.Description?.Trim()));
        }

        if (request.IsApproved)
        {
            Result<string> stockResult = await CostSlipStockHelper.ApplyStockEffectsAsync(
                slip,
                request.CostingMethod,
                productMovementRepository,
                ledgerPoster,
                productRepository,
                cancellationToken);

            if (!stockResult.IsSuccessful)
            {
                return Result<CostSlipCreateResult>.Failure(
                    stockResult.ErrorMessages?.FirstOrDefault() ?? "Stok hareketleri uygulanamadı.");
            }

            slip.Approve();
            if (!isRestored)
            {
                await costSlipRepository.AddAsync(slip, cancellationToken);
            }

            return Result<CostSlipCreateResult>.Succeed(new CostSlipCreateResult(
                slip.Id,
                "Maliyet pusulası onaylı olarak kaydedildi; stok hareketleri oluşturuldu."));
        }

        if (!isRestored)
        {
            await costSlipRepository.AddAsync(slip, cancellationToken);
        }

        return Result<CostSlipCreateResult>.Succeed(new CostSlipCreateResult(
            slip.Id,
            "Maliyet pusulası taslak olarak kaydedildi. Onaylanınca stok hareketleri oluşturulacak."));
    }
}