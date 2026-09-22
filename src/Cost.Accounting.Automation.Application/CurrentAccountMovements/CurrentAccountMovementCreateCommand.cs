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

[Permission("current_account_movement:create")]
public sealed record CurrentAccountMovementCreateCommand(
    CurrentAccountType CurrentAccountType,
    Guid? CustomerId,
    Guid? SupplierId,
    DateOnly Date,
    CurrentAccountMovementType MovementType,
    string? DocumentNo,
    decimal Debit,
    decimal Credit,
    string Description) : IRequest<Result<string>>;

public sealed class CurrentAccountMovementCreateCommandValidator : AbstractValidator<CurrentAccountMovementCreateCommand>
{
    public CurrentAccountMovementCreateCommandValidator()
    {
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

internal sealed class CurrentAccountMovementCreateCommandHandler(
    ICurrentAccountMovementRepository currentAccountMovementRepository,
    ICustomerRepository customerRepository,
    ISupplierRepository supplierRepository,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<CurrentAccountMovementCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CurrentAccountMovementCreateCommand request, CancellationToken cancellationToken)
    {
        IdentityId? customerId = request.CustomerId.HasValue ? new IdentityId(request.CustomerId.Value) : null;
        IdentityId? supplierId = request.SupplierId.HasValue ? new IdentityId(request.SupplierId.Value) : null;

        if (request.CurrentAccountType == CurrentAccountType.Customer && customerId != null)
        {
            bool customerExists = await customerRepository.AnyAsync(c => c.Id == customerId, cancellationToken);
            if (!customerExists)
            {
                return Result<string>.Failure("Seçilen müşteri bulunamadı.");
            }
        }
        else if (request.CurrentAccountType == CurrentAccountType.Supplier && supplierId != null)
        {
            bool supplierExists = await supplierRepository.AnyAsync(s => s.Id == supplierId, cancellationToken);
            if (!supplierExists)
            {
                return Result<string>.Failure("Seçilen tedarikçi bulunamadı.");
            }
        }

        string? documentNo = string.IsNullOrWhiteSpace(request.DocumentNo) ? null : request.DocumentNo.Trim();

        IdentityId? accountId = request.CurrentAccountType == CurrentAccountType.Customer ? customerId : supplierId;

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
            includeDeleted: true,
            cancellationToken: cancellationToken);

        if (duplicate is not null)
        {
            string typeName = request.MovementType == CurrentAccountMovementType.Collection
                ? "Tahsilat"
                : "Ödeme";

            decimal amount = request.Debit > 0 ? request.Debit : request.Credit;

            if (!duplicate.IsDeleted)
            {
                return Result<string>.Failure(
                    $"Aynı {typeName} kaydı zaten mevcut ({request.Date:dd.MM.yyyy} - {amount:n2} TL). Kopya kayıt oluşturulmadı.");
            }

            if (documentNo is null)
            {
                documentNo = string.IsNullOrEmpty(duplicate.DocumentNo)
                    ? await CurrentAccountMovementHelper.GenerateNextAsync(currentAccountMovementRepository, request.MovementType, cancellationToken)
                    : duplicate.DocumentNo;
            }

            duplicate.Restore();
            duplicate.SetStatus(true);
            duplicate.Update(
                date: request.Date,
                movementType: request.MovementType,
                documentNo: documentNo,
                debit: request.Debit,
                credit: request.Credit,
                description: new Description(request.Description ?? string.Empty));

            return Result<string>.Succeed($"Silinmiş olan aynı {typeName} kaydı geri getirilip güncellendi.");
        }

        if (documentNo is null && CurrentAccountMovement.IsManualPaymentType(request.MovementType))
        {
            documentNo = await CurrentAccountMovementHelper.GenerateNextAsync(currentAccountMovementRepository, request.MovementType, cancellationToken);
        }

        CurrentAccountMovement movement = new(
            currentAccountType: request.CurrentAccountType,
            customerId: customerId,
            supplierId: supplierId,
            date: request.Date,
            movementType: request.MovementType,
            documentNo: documentNo,
            debit: request.Debit,
            credit: request.Credit,
            description: new Description(request.Description ?? string.Empty));

        await currentAccountMovementRepository.AddAsync(movement, cancellationToken);

        return Result<string>.Succeed("Cari hareket başarıyla kaydedildi.");
    }
}
