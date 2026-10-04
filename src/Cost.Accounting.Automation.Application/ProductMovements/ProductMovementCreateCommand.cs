using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ProductMovements;

[Permission("stock_movement:create")]
public sealed record ProductMovementCreateCommand(
    Guid ProductId,
    ProductMovementType MovementType,
    decimal Quantity,
    decimal? UnitPrice,
    DateOnly Date,
    string? ReferenceNo,
    string Description,
    ProductMovementReason Reason = ProductMovementReason.General) : IRequest<Result<string>>;

public sealed class ProductMovementCreateCommandValidator : AbstractValidator<ProductMovementCreateCommand>
{
    public ProductMovementCreateCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün seçilmelidir.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar sıfırdan büyük olmalıdır.");

        When(x => x.UnitPrice.HasValue, () =>
        {
            RuleFor(x => x.UnitPrice!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Birim fiyat negatif olamaz.");
        });
    }
}

internal sealed class ProductMovementCreateCommandHandler(
    IProductMovementRepository productMovementRepository,
    IProductRepository productRepository,
    IChartOfAccountLedgerPoster ledgerPoster,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<ProductMovementCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductMovementCreateCommand request, CancellationToken cancellationToken)
    {
        IdentityId productId = new(request.ProductId);
        Product? product = await productRepository.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product is null)
        {
            return Result<string>.Failure("Seçilen ürün bulunamadı.");
        }

        DateOnly? lastMovementDate = await productMovementRepository
            .GetAllWithAudit()
            .Select(m => m.Entity.Date)
            .OrderByDescending(d => d)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastMovementDate.HasValue && request.Date < lastMovementDate.Value)
        {
            return Result<string>.Failure(
                $"En son stok hareketi {lastMovementDate.Value:dd.MM.yyyy} tarihli olduğundan önceki bir tarihe stok hareketi kaydedilemez.");
        }

        string referenceNo = request.ReferenceNo?.Trim() ?? string.Empty;

        string? duplicateKey = ProductMovement.BuildDuplicateKey(
            productId,
            request.MovementType,
            request.Quantity,
            request.UnitPrice,
            request.Date,
            referenceNo);

        ProductMovement? duplicate = await duplicateCheckService.FindDuplicateAsync<ProductMovement>(
            duplicateKey,
            includeDeleted: true,
            cancellationToken: cancellationToken);

        if (duplicate is not null)
        {
            return Result<string>.Failure("Aynı ürün, miktar, birim fiyat, tarih ve referans numarası ile bir stok hareketi zaten mevcut.");
        }

        if (request.MovementType == ProductMovementType.Output)
        {
            List<ProductMovement> movements = await StockIssueCostingHelper.LoadMovementsAsync(
                [productId],
                productMovementRepository,
                cancellationToken);

            decimal available = StockIssueCostingHelper.ComputeAvailableQuantity(
                movements,
                productId,
                request.Date);

            if (request.Quantity > available)
            {
                return Result<string>.Failure(
                    $"'{product.Name.Value}' için bu tarihe kadar yeterli giriş (stok) yok. Mevcut: {available:n2}, istenen: {request.Quantity:n2}.");
            }

            // Çıkış FIFO ile giriş katmanlarına bölünür: ilk giren tamamen
            // tüketilir, kalan miktar sonraki girişten alınır. Her katman için
            // AYRI hareket yazılır ve fiyatı o katmanın GİRİŞ fiyatıdır.
            //
            // Tek bir fiyatla yazılsaydı o fiyat hiçbir girişe uymazdı; stok
            // hareketi listesi raporu fiyatı grup anahtarı olarak kullandığı için
            // girişi olmayan, bakiyesi eksi satırlar oluşurdu. Kullanıcının
            // ekranda yazdığı fiyat bilgilendirme amaçlıdır; çıkışın fiyatı
            // daima giriş fiyatından gelir.
            List<(decimal Quantity, decimal UnitPrice)> layers =
                StockIssueCostingHelper.BuildConsumptionLayers(
                    movements, productId, request.Quantity, StockCostingMethod.Fifo, request.Date);

            if (layers.Count > 0)
            {
                foreach ((decimal layerQuantity, decimal layerUnitPrice) in layers)
                {
                    ProductMovement layerMovement = new(
                        productId: productId,
                        movementType: ProductMovementType.Output,
                        quantity: layerQuantity,
                        unitPrice: new Price(layerUnitPrice),
                        date: request.Date,
                        referenceNo: referenceNo,
                        description: new Description(request.Description ?? string.Empty),
                        reason: request.Reason);

                    await productMovementRepository.AddAsync(layerMovement, cancellationToken);

                    if (product.ChartOfAccountId is { } layerAccountId && layerUnitPrice > 0)
                    {
                        decimal layerAmount = Math.Round(layerQuantity * layerUnitPrice, 2);

                        await ledgerPoster.PostAsync(
                            layerAccountId, 0, layerAmount, "StokCikisi", layerMovement.Id, cancellationToken);
                    }
                }

                return layers.Count == 1
                    ? Result<string>.Succeed("Stok çıkışı başarıyla kaydedildi.")
                    : Result<string>.Succeed(
                        $"Stok çıkışı başarıyla kaydedildi ({layers.Count} farklı giriş fiyatından karşılandı).");
            }
        }

        ProductMovement movement = new(
            productId: productId,
            movementType: request.MovementType,
            quantity: request.Quantity,
            unitPrice: request.UnitPrice.HasValue ? new Price(request.UnitPrice.Value) : null,
            date: request.Date,
            referenceNo: request.ReferenceNo?.Trim(),
            description: new Description(request.Description ?? string.Empty),
            reason: request.Reason);

        await productMovementRepository.AddAsync(movement, cancellationToken);

        if (product.ChartOfAccountId is { } accountId && request.UnitPrice.HasValue && request.UnitPrice.Value > 0)
        {
            decimal amount = Math.Round(request.Quantity * request.UnitPrice.Value, 2);

            if (request.MovementType == ProductMovementType.Input)
            {
                await ledgerPoster.PostAsync(accountId, amount, 0, "StokGirisi", movement.Id, cancellationToken);
            }
            else
            {
                await ledgerPoster.PostAsync(accountId, 0, amount, "StokCikisi", movement.Id, cancellationToken);
            }
        }

        string actionName = request.MovementType == ProductMovementType.Input ? "Stok girişi" : "Stok çıkışı";
        return Result<string>.Succeed($"{actionName} başarıyla kaydedildi.");
    }
}
