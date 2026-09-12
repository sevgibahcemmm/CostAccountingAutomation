using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;
internal sealed class CompanyRepository : AuditableRepository<Company, ApplicationDbContext>, ICompanyRepository    
{
    public CompanyRepository(ApplicationDbContext context) : base(context)
    {
    }
}