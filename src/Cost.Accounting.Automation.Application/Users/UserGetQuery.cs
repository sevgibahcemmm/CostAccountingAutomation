using Microsoft.EntityFrameworkCore;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Users;
[Permission("user:view")]
public sealed record UserGetQuery(
    Guid Id) : IRequest<Result<UserDto>>;

internal sealed class UserGetQueryHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    ICompanyRepository CompanyRepository) : IRequestHandler<UserGetQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(UserGetQuery request, CancellationToken cancellationToken)
    {
        var res = await userRepository
            .GetAllWithAudit()
            .MapTo(roleRepository.GetAll(), CompanyRepository.GetAll())
            .Where(i => i.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (res is null)
        {
            return Result<UserDto>.Failure("Kullanıcı bulunamadı");
        }

        return res;
    }
}