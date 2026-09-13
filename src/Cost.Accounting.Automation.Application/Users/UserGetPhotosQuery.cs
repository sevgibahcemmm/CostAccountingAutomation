using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Photos;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Users;

[Permission("user:view")]
public sealed record UserGetPhotosQuery(Guid UserId) : IRequest<Result<List<PhotoDto>>>;

internal sealed class UserGetPhotosQueryHandler(
    IPhotoRepository photoRepository) : IRequestHandler<UserGetPhotosQuery, Result<List<PhotoDto>>>
{
    public async Task<Result<List<PhotoDto>>> Handle(UserGetPhotosQuery request, CancellationToken cancellationToken)
    {
        var photos = await photoRepository
            .Where(p => p.UserId == new IdentityId(request.UserId))
            .OrderBy(p => p.IsDefault ? 0 : 1)
            .ThenBy(p => p.CreatedAt)
            .Select(p => new PhotoDto(p.FileName, p.ContentType, p.Path, p.IsDefault))
            .ToListAsync(cancellationToken);

        return photos;
    }
}