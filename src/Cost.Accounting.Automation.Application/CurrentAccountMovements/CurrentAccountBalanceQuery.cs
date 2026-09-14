using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.CurrentAccountMovements;

public enum CurrentAccountBalanceScope
{
    All = 0,
    Debtors = 1,
    Creditors = 2
}

[Permission("current_account_movement:view")]
public sealed record CurrentAccountBalanceQuery(CurrentAccountBalanceScope Scope = CurrentAccountBalanceScope.All) : IRequest<List<CurrentAccountBalanceDto>>
{
    public CurrentAccountBalanceQuery() : this(CurrentAccountBalanceScope.All) { }
}

public sealed class CurrentAccountBalanceDto
{
    [Column("Id", IsVisible = false)]
    public Guid Id { get; set; }

    [Column("Cari Türü", IsVisible = false)]
    public CurrentAccountType AccountType { get; set; }

    [Column("Cari Türü", Order = 10, Width = 80, Alignment = "Center")]
    public string AccountTypeName => AccountType == CurrentAccountType.Customer ? "Müşteri" : "Tedarikçi";

    [Column("Cari Adı", Order = 20, Width = 220)]
    public string AccountName { get; set; } = default!;

    [Column("Toplam Borç", Order = 30, Width = 120, Format = "n2", Alignment = "Right")]
    public decimal TotalDebit { get; set; }

    [Column("Toplam Alacak", Order = 40, Width = 120, Format = "n2", Alignment = "Right")]
    public decimal TotalCredit { get; set; }

    [Column("Bakiye", Order = 50, Width = 120, Format = "n2", Alignment = "Right")]
    public decimal Balance => TotalDebit - TotalCredit;
}

internal sealed class CurrentAccountBalanceQueryHandler(
    ICurrentAccountMovementRepository currentAccountMovementRepository,
    ICustomerRepository customerRepository,
    ISupplierRepository supplierRepository) : IRequestHandler<CurrentAccountBalanceQuery, List<CurrentAccountBalanceDto>>
{
    public async Task<List<CurrentAccountBalanceDto>> Handle(CurrentAccountBalanceQuery request, CancellationToken cancellationToken)
    {
        List<CurrentAccountMovement> movements = await currentAccountMovementRepository.GetAll()
            .ToListAsync(cancellationToken);

        var customerNameLookup = await customerRepository.GetAll()
            .Select(c => new { Key = c.Id.Value, Value = c.Name.Value })
            .ToDictionaryAsync(c => c.Key, c => c.Value, cancellationToken);

        var supplierNameLookup = await supplierRepository.GetAll()
            .Select(s => new { Key = s.Id.Value, Value = s.Name.Value })
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        List<CurrentAccountBalanceDto> result = [];

        var customerGroups = movements
            .Where(m => m.CurrentAccountType == CurrentAccountType.Customer && m.CustomerId != null)
            .GroupBy(m => m.CustomerId!.Value)
            .ToList();

        foreach (var g in customerGroups)
        {
            decimal debit = g.Sum(m => m.Debit);
            decimal credit = g.Sum(m => m.Credit);
            decimal balance = debit - credit;

            if (request.Scope == CurrentAccountBalanceScope.Debtors && balance <= 0) continue;
            if (request.Scope == CurrentAccountBalanceScope.Creditors && balance >= 0) continue;

            customerNameLookup.TryGetValue(g.Key, out string? name);

            result.Add(new CurrentAccountBalanceDto
            {
                Id = g.Key,
                AccountType = CurrentAccountType.Customer,
                AccountName = name ?? "-",
                TotalDebit = debit,
                TotalCredit = credit
            });
        }

        var supplierGroups = movements
            .Where(m => m.CurrentAccountType == CurrentAccountType.Supplier && m.SupplierId != null)
            .GroupBy(m => m.SupplierId!.Value)
            .ToList();

        foreach (var g in supplierGroups)
        {
            decimal debit = g.Sum(m => m.Debit);
            decimal credit = g.Sum(m => m.Credit);
            decimal balance = debit - credit;

            if (request.Scope == CurrentAccountBalanceScope.Debtors && balance <= 0) continue;
            if (request.Scope == CurrentAccountBalanceScope.Creditors && balance >= 0) continue;

            supplierNameLookup.TryGetValue(g.Key, out string? name);

            result.Add(new CurrentAccountBalanceDto
            {
                Id = g.Key,
                AccountType = CurrentAccountType.Supplier,
                AccountName = name ?? "-",
                TotalDebit = debit,
                TotalCredit = credit
            });
        }

        return result;
    }
}