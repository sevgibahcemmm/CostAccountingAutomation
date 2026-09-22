using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Products.ProductUnitTypes;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products.ProductUnitTypes;

[Permission("product:update")]
public sealed record ProductUnitTypeUpdateCommand(
    Guid Id,
    string Name,
    bool IsActive) : IRequest<Result<string>>;

public sealed class ProductUnitTypeUpdateCommandValidator : AbstractValidator<ProductUnitTypeUpdateCommand>
{
    public ProductUnitTypeUpdateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Geçerli bir ID girin");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Geçerli bir birim adı girin")
            .MaximumLength(120).WithMessage("Birim adı en fazla 120 karakter olabilir");
    }
}

internal sealed class ProductUnitTypeUpdateCommandHandler(
    IProductUnitTypeRepository unitTypeRepository,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<ProductUnitTypeUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductUnitTypeUpdateCommand request, CancellationToken cancellationToken)
    {
        IdentityId unitTypeId = new(request.Id);

        var unitType = await unitTypeRepository.GetByExpressionWithTrackingAsync(
            p => p.Id == unitTypeId,
            cancellationToken);

        if (unitType is null)
        {
            return Result<string>.Failure("Birim cinsi bulunamadı");
        }

        string? duplicateKey = ProductUnitType.BuildDuplicateKey(request.Name);

        ProductUnitType? nameDuplicate = await duplicateCheckService.FindDuplicateAsync<ProductUnitType>(
            duplicateKey,
            excludeId: request.Id,
            cancellationToken: cancellationToken);

        if (nameDuplicate is not null)
        {
            return Result<string>.Failure("Bu birim cinsi başka bir kayıt tarafından kullanılıyor");
        }

        unitType.SetName(new Name(request.Name));
        unitType.SetStatus(request.IsActive);

        unitTypeRepository.Update(unitType);

        return "Birim cinsi başarıyla güncellendi";
    }
}