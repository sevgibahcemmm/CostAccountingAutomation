using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Users;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Users;
[Permission("user:view")]
public sealed record UserGetAllQuery(
    bool OnlyDeleted = false) : IRequest<IQueryable<UserDto>>
{
    public UserGetAllQuery() : this(false) { }
}

internal sealed class UserGetAllQueryHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IClaimContext claimContext,
    ICompanyRepository CompanyRepository) : IRequestHandler<UserGetAllQuery, IQueryable<UserDto>>
{
    public Task<IQueryable<UserDto>> Handle(UserGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EntityWithAuditDto<User>> source = request.OnlyDeleted
            ? userRepository.GetAllWithAuditIncludingDeleted().Where(i => i.Entity.IsDeleted)
            : userRepository.GetAllWithAudit();

        var res = source
            .MapTo(roleRepository
            .GetAll(), CompanyRepository.GetAll());

        if (claimContext.GetRoleName() != "sys_admin")
        {
            res = res.Where(i => i.CompanyId == claimContext.GetCompanyId());
        }

        return Task.FromResult(res);
    }
}