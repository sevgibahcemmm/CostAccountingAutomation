using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.Suppliers;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.CurrentAccountMovements;

[Permission("current_account_movement:update")]
public sealed record CurrentAccountMovementUpdateCommand(
    Guid Id,
    CurrentAccountType CurrentAccountType,
    Guid? CustomerId,
    Guid? SupplierId,
    DateOnly Date,
    CurrentAccountMovementType MovementType,
    string? DocumentNo,
    decimal Debit,
    decimal Credit,
    string Description) : IRequest<Result<string>>;

public sealed class CurrentAccountMovementUpdateCommandValidator : AbstractValidator<CurrentAccountMovementUpdateCommand>
{
    public CurrentAccountMovementUpdateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Geçerli bir hareket kimliği girin.");

        When(x => x.CurrentAccountType == CurrentAccountType.Customer, () =>
        {
            RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Müşteri seçilmelidir.");
        });

        When(x => x.CurrentAccountType == CurrentAccountType.Supplier, () =>
        {
            RuleFor(x => x.SupplierId).NotEmpty().WithMessage("Tedarikçi seçilmelidir.");
        });

        RuleFor(x => x.Debit)
            .GreaterThanOrEqualTo(0).WithMessage("Borç tutarı negatif olamaz.");

        RuleFor(x => x.Credit)
            .GreaterThanOrEqualTo(0).WithMessage("Alacak tutarı negatif olamaz.");

        RuleFor(x => x)
            .Must(x => x.Debit > 0 || x.Credit > 0)
            .WithMessage("Borç veya alacak tutarlarından en az biri sıfırdan büyük olmalıdır.");
    }
}

internal sealed class CurrentAccountMovementUpdateCommandHandler(
    ICurrentAccountMovementRepository currentAccountMovementRepository,
    ICustomerRepository customerRepository,
    ISupplierRepository supplierRepository,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<CurrentAccountMovementUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CurrentAccountMovementUpdateCommand request, CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);
        CurrentAccountMovement? movement = await currentAccountMovementRepository.GetByExpressionWithTrackingAsync(
            m => m.Id == id,
            cancellationToken);

        if (movement is null)
        {
            return Result<string>.Failure("Cari hareket bulunamadı veya silinmiş.");
        }

        if (request.CurrentAccountType == CurrentAccountType.Customer)
        {
            IdentityId customerId = new(request.CustomerId!.Value);
            bool customerExists = await customerRepository.AnyAsync(c => c.Id == customerId, cancellationToken);
            if (!customerExists)
            {
                return Result<string>.Failure("Seçilen müşteri bulunamadı.");
            }

            movement.SetCustomer(customerId);
        }
        else if (request.CurrentAccountType == CurrentAccountType.Supplier)
        {
            IdentityId supplierId = new(request.SupplierId!.Value);
            bool supplierExists = await supplierRepository.AnyAsync(s => s.Id == supplierId, cancellationToken);
            if (!supplierExists)
            {
                return Result<string>.Failure("Seçilen tedarikçi bulunamadı.");
            }

            movement.SetSupplier(supplierId);
        }

        string? documentNo = string.IsNullOrWhiteSpace(request.DocumentNo) ? null : request.DocumentNo.Trim();

        if (CurrentAccountMovement.IsManualPaymentType(request.MovementType))
        {
            IdentityId? accountId = request.CurrentAccountType == CurrentAccountType.Customer
                ? new IdentityId(request.CustomerId!.Value)
                : new IdentityId(request.SupplierId!.Value);

            string? duplicateKey = CurrentAccountMovement.BuildDuplicateKey(
                request.CurrentAccountType,
                accountId,
                request.MovementType,
                request.Date,
                request.Debit,
                request.Credit,
                documentNo);

            CurrentAccountMovement? duplicate = await duplicateCheckService.FindDuplicateAsync<CurrentAccountMovement>(
                duplicateKey,
                excludeId: request.Id,
                includeDeleted: true,
                cancellationToken: cancellationToken);

            if (duplicate is not null)
            {
                string typeName = request.MovementType == CurrentAccountMovementType.Collection
                    ? "Tahsilat"
                    : "Ödeme";

                decimal amount = request.Debit > 0 ? request.Debit : request.Credit;

                return Result<string>.Failure(
                    $"Bu ölçütlerde başka bir {typeName} kaydı zaten mevcut ({request.Date:dd.MM.yyyy} - {amount:n2} TL). Kopya kayıt oluşturulmadı.");
            }

            if (documentNo is null)
            {
                documentNo = string.IsNullOrEmpty(movement.DocumentNo)
                    ? await CurrentAccountMovementHelper.GenerateNextAsync(currentAccountMovementRepository, request.MovementType, cancellationToken)
                    : movement.DocumentNo;
            }
        }

        movement.Update(
            date: request.Date,
            movementType: request.MovementType,
            documentNo: documentNo,
            debit: request.Debit,
            credit: request.Credit,
            description: new Description(request.Description ?? string.Empty));

        currentAccountMovementRepository.Update(movement);

        return Result<string>.Succeed("Cari hareket başarıyla güncellendi.");
    }
}