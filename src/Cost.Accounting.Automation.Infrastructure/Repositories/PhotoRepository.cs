using Cost.Accounting.Automation.Domain.Photos;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class PhotoRepository : AuditableRepository<Photo, ApplicationDbContext>, IPhotoRepository
{
    public PhotoRepository(ApplicationDbContext context) : base(context)
    {
    }
}