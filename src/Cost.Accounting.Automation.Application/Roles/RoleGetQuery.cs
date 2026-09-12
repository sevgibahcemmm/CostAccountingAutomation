using Microsoft.EntityFrameworkCore;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Roles;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Roles;
[Permission("role:view")]
public sealed record RoleGetQuery(
    Guid Id) : IRequest<Result<RoleDto>>;

internal sealed class RoleGetQueryHandler(
    IRoleRepository roleRepository) : IRequestHandler<RoleGetQuery, Result<RoleDto>>
{
    public async Task<Result<RoleDto>> Handle(RoleGetQuery request, CancellationToken cancellationToken)
    {
        var res = await roleRepository
            .GetAllWithAudit()
            .MapToGet()
            .Where(p => p.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (res is null)
        {
            return Result<RoleDto>.Failure("Rol bulunamadı");
        }

        return res;
    }
}