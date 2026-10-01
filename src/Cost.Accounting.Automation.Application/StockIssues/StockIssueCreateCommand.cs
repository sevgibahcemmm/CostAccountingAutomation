using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.StockIssues;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.StockIssues;

[Permission("stock_issue:create")]
public sealed record StockIssueCreateCommand(
    StockIssueType IssueType,
    Guid SourceWarehouseId,
    Guid TargetAccountId,
    DateOnly Date,
    string DocumentNumber,
    StockCostingMethod CostingMethod,
    string? Description,
    IReadOnlyCollection<StockIssueCreateLine> Lines) : IRequest<Result<string>>;

public sealed record StockIssueCreateLine(Guid ProductId, decimal Quantity, string? Description);

public sealed class StockIssueCreateCommandValidator : AbstractValidator<StockIssueCreateCommand>
{
    public StockIssueCreateCommandValidator()
    {
        RuleFor(x => x.SourceWarehouseId)
            .NotEmpty().WithMessage("Kaynak depo seçilmelidir.");

        RuleFor(x => x.TargetAccountId)
            .NotEmpty().WithMessage("Hedef hesap seçilmelidir.");

        RuleFor(x => x.Date)
            .NotEqual(default(DateOnly)).WithMessage("Tarih seçilmelidir.");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("Belge numarası zorunludur.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("En az bir ürün satırı eklenmelidir.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId)
                .NotEmpty().WithMessage("Ürün seçilmelidir.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0).WithMessage("Miktar sıfırdan büyük olmalıdır.");
        });
    }
}

internal sealed class StockIssueCreateCommandHandler(
    IStockIssueRepository stockIssueRepository,
    IChartOfAccountRepository chartOfAccountRepository,
    IProductRepository productRepository,
    IProductMovementRepository productMovementRepository,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<StockIssueCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(StockIssueCreateCommand request, CancellationToken cancellationToken)
    {
        bool isConsumption = request.IssueType == StockIssueType.Consumption;

        List<ChartOfAccount> accounts = await chartOfAccountRepository.GetAllIncludingDeletedAsync(cancellationToken);

        ChartOfAccount? sourceWarehouse = accounts.FirstOrDefault(a =>
            a.Id.Value == request.SourceWarehouseId && !a.IsDeleted && a.IsActive);

        if (sourceWarehouse is null || sourceWarehouse.Type != ChartOfAccountType.Warehouse)
        {
            return Result<string>.Failure("Kaynak depo bulunamadı veya geçerli bir depo değil.");
        }

        string expectedCode = isConsumption ? "150" : "150.98";
        if (!string.Equals(sourceWarehouse.Code.Value, expectedCode, StringComparison.OrdinalIgnoreCase))
        {
            return Result<string>.Failure(isConsumption
                ? "Tüketim yalnızca 150 (İlk Madde ve Malzeme) deposundan yapılabilir."
                : "Atölye transferi yalnızca 150.98 (Üretime Yönelik) deposundan yapılabilir.");
        }

        ChartOfAccount? target = accounts.FirstOrDefault(a =>
            a.Id.Value == request.TargetAccountId && !a.IsDeleted && a.IsActive);

        if (target is null)
        {
            return Result<string>.Failure("Hedef hesap bulunamadı.");
        }

        if (isConsumption)
        {
            if (target.Type != ChartOfAccountType.ConsumptionUnit)
            {
                return Result<string>.Failure("Tüketimde hedef hesap, 900 (Tüketimler) altındaki bir tüketim birimi olmalıdır.");
            }
        }
        else if (target.Type != ChartOfAccountType.Workshop)
        {
            return Result<string>.Failure("Atölye transferinde hedef hesap, 150.55 altındaki bir atölye hesabı olmalıdır.");
        }

        string documentNumber = request.DocumentNumber.Trim();

        string? duplicateKey = StockIssue.BuildDuplicateKey(documentNumber, request.IssueType);

        StockIssue? duplicate = await duplicateCheckService.FindDuplicateAsync<StockIssue>(
            duplicateKey,
            includeDeleted: true,
            cancellationToken: cancellationToken);

        if (duplicate is not null && !duplicate.IsDeleted)
        {
            return Result<string>.Failure($"'{documentNumber}' belge numarası zaten kullanılıyor.");
        }

        bool isRestored = duplicate is not null;

        List<IdentityId> requestedProductIds = request.Lines
            .Select(l => new IdentityId(l.ProductId))
            .Distinct()
            .ToList();
        HashSet<IdentityId> productIdSet = requestedProductIds.ToHashSet();

        List<Product> products = await productRepository.GetAll()
            .Where(p => productIdSet.Contains(p.Id))
            .ToListAsync(cancellationToken);

        Dictionary<Guid, Product> productMap = products.ToDictionary(p => p.Id.Value);

        // Aynı ürün aynı belgede birden fazla satırda kullanılamaz. Satırlar
        // sessizce birleştirilmez; kullanıcı hatalı satırı düzeltsin.
        List<IGrouping<Guid, StockIssueCreateLine>> duplicateLines = request.Lines
            .GroupBy(l => l.ProductId)
            .Where(g => g.Count() > 1)
            .ToList();

        if (duplicateLines.Count > 0)
        {
            string duplicateNames = string.Join(", ", duplicateLines.Select(g =>
                productMap.TryGetValue(g.Key, out Product? dupProduct)
                    ? $"'{dupProduct.Name.Value}'"
                    : g.Key.ToString()));

            return Result<string>.Failure(
                $"Aynı ürün tek belgede yalnızca bir satırda kullanılabilir. Tekrarlanan ürünler: {duplicateNames}.");
        }

        foreach (StockIssueCreateLine line in request.Lines)
        {
            if (!productMap.TryGetValue(line.ProductId, out Product? product))
            {
                return Result<string>.Failure("Seçilen ürünlerden biri bulunamadı.");
            }

            if (!product.IsActive)
            {
                return Result<string>.Failure($"'{product.Name.Value}' ürünü aktif değil.");
            }

            if (product.WarehouseId.Value != request.SourceWarehouseId)
            {
                return Result<string>.Failure($"'{product.Name.Value}' ürünü seçilen depoya ait değil.");
            }
        }

        Dictionary<IdentityId, decimal> requestedQuantities = request.Lines
            .GroupBy(l => new IdentityId(l.ProductId))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        List<ProductMovement> movements = await StockIssueCostingHelper.LoadMovementsAsync(
            requestedProductIds,
            productMovementRepository,
            cancellationToken);

        foreach (KeyValuePair<IdentityId, decimal> requested in requestedQuantities)
        {
            decimal available = StockIssueCostingHelper.ComputeAvailableQuantity(
                movements,
                requested.Key,
                request.Date);

            if (requested.Value > available)
            {
                string productName = productMap.TryGetValue(requested.Key.Value, out Product? p)
                    ? p.Name.Value
                    : requested.Key.Value.ToString();

                return Result<string>.Failure(
                    $"'{productName}' için bu tarihe kadar yeterli giriş (stok) yok. Mevcut: {available:n2}, istenen: {requested.Value:n2}.");
            }
        }

        Dictionary<IdentityId, decimal> costMap = StockIssueCostingHelper.BuildUnitCostMap(
            movements,
            requestedQuantities,
            request.CostingMethod,
            request.Date);

        StockIssue issue;
        if (duplicate is not null)
        {
            duplicate.Restore();
            duplicate.SetDescription(new Description(request.Description?.Trim() ?? string.Empty));
            issue = duplicate;
        }
        else
        {
            issue = new StockIssue(
                issueType: request.IssueType,
                documentNumber: documentNumber,
                date: request.Date,
                sourceWarehouseId: new IdentityId(request.SourceWarehouseId),
                targetAccountId: new IdentityId(request.TargetAccountId),
                costingMethod: request.CostingMethod,
                description: new Description(request.Description?.Trim() ?? string.Empty));
        }

        List<StockIssueLine> lines = [];

        List<StockIssueCreateLine> mergedLines = request.Lines
            .GroupBy(l => l.ProductId)
            .Select(g => new StockIssueCreateLine(
                g.Key,
                g.Sum(x => x.Quantity),
                g.First().Description))
            .ToList();

        foreach (StockIssueCreateLine line in mergedLines)
        {
            decimal unitCost = costMap.TryGetValue(new IdentityId(line.ProductId), out decimal cost) ? cost : 0m;

            lines.Add(new StockIssueLine(
                stockIssueId: issue.Id,
                productId: new IdentityId(line.ProductId),
                quantity: line.Quantity,
                unitCost: new Price(unitCost),
                description: new Description(line.Description?.Trim() ?? string.Empty)));
        }

        issue.ReplaceLines(lines);

        if (!isRestored)
        {
            await stockIssueRepository.AddAsync(issue, cancellationToken);
        }

        // Belge daima TASLAK olarak kaydedilir. Stok ve yevmiye hareketleri
        // ONAYDA uretilir (StockIssueApproveCommand); boylece onaylanmamis bir
        // transfer/tuketim stogu etkilemez ve maliyet pusulasinda tuketilemez.
        string actionName = isConsumption ? "Tuketim" : "Atolye transferi";
        return Result<string>.Succeed($"{actionName} belgesi taslak olarak kaydedildi.");
    }
}
