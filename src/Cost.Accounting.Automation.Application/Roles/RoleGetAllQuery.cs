using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Roles;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Roles;
[Permission("role:view")]
public sealed record RoleGetAllQuery : IRequest<IQueryable<RoleDto>>;

internal sealed class RoleGetAllQueryHandler(
    IRoleRepository roleRepository) : IRequestHandler<RoleGetAllQuery, IQueryable<RoleDto>>
{
    public Task<IQueryable<RoleDto>> Handle(RoleGetAllQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(roleRepository.GetAllWithAudit().MapTo().AsQueryable());
}