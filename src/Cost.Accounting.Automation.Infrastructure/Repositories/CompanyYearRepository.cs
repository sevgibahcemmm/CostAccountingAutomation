using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.AccountingYears;
using Cost.Accounting.Automation.Domain.AccountingYears.ValueObjects;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class CompanyYearRepository : ICompanyYearRepository
{
    private readonly MasterDbContext _context;

    public CompanyYearRepository(MasterDbContext context)
    {
        _context = context;
    }

    public Task<List<CompanyYear>> GetAllAsync(CancellationToken cancellationToken = default)
        => Years()
            .OrderBy(cy => cy.Year)
            .ThenBy(cy => cy.CompanyId)
            .ToListAsync(cancellationToken);

    public Task<List<CompanyYear>> GetAllByCompanyAsync(
        IdentityId companyId,
        CancellationToken cancellationToken = default)
        => Years()
            .Where(cy => cy.CompanyId == companyId)
            .OrderByDescending(cy => cy.Year)
            .ToListAsync(cancellationToken);

    public Task<List<CompanyYear>> GetAllOpenAsync(CancellationToken cancellationToken = default)
        => Years()
            .Where(cy => !cy.IsClosed)
            .OrderBy(cy => cy.Year)
            .ThenBy(cy => cy.CompanyId)
            .ToListAsync(cancellationToken);

    public Task<CompanyYear?> GetByIdAsync(IdentityId id, CancellationToken cancellationToken = default)
        => Years()
            .FirstOrDefaultAsync(cy => cy.Id == id, cancellationToken);

    public Task<CompanyYear?> GetByCompanyAndYearAsync(
        IdentityId companyId,
        int year,
        CancellationToken cancellationToken = default)
        => Years()
            .FirstOrDefaultAsync(
                cy => cy.CompanyId == companyId && cy.Year == new Year(year),
                cancellationToken);

    public Task<CompanyYear?> GetByDatabaseNameAsync(
        string databaseName,
        CancellationToken cancellationToken = default)
        => Years()
            .FirstOrDefaultAsync(
                cy => cy.DatabaseName == new DatabaseName(databaseName),
                cancellationToken);

    public Task<bool> DatabaseNameExistsAsync(
        string databaseName,
        CancellationToken cancellationToken = default)
        => Years()
            .AnyAsync(cy => cy.DatabaseName == new DatabaseName(databaseName), cancellationToken);

    public async Task AddAsync(CompanyYear companyYear, CancellationToken cancellationToken = default)
        => await _context.Set<CompanyYear>().AddAsync(companyYear, cancellationToken);

    public void Update(CompanyYear companyYear)
        => _context.Set<CompanyYear>().Update(companyYear);

    private IQueryable<CompanyYear> Years()
        => _context.Set<CompanyYear>().AsNoTrackingWithIdentityResolution();
}
