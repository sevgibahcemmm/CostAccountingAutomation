using FluentValidation;
using GenericRepository;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Domain.Users.ValueObjects;
using Cost.Accounting.Automation.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Auth;
public sealed record LoginCommand(
    string EmailOrUserName,
    string Password,
    Guid? CompanyId = null) : IRequest<Result<LoginCommandResponse>>;


public sealed record LoginCommandResponse
{
    public string? Token { get; set; }
}
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(p => p.EmailOrUserName).NotEmpty().WithMessage("Geçerli bir mail ya da kullanıcı adı girin");
        RuleFor(p => p.Password).NotEmpty().WithMessage("Geçerli bir şifre girin");
    }
}

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IJwtProvider jwtProvider) : IRequestHandler<LoginCommand, Result<LoginCommandResponse>>
{
    public async Task<Result<LoginCommandResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Kullanıcı adı/e-posta eşleşmesi veritabanı collation'ına değil koda bağlıdır;
        // bkz. LoginNameMatcher. Böylece sunucu _CS collation ile kurulmuş olsa bile
        // "Admin" ile "admin" aynı kullanıcıya gider.
        string identifier = LoginNameMatcher.Clean(request.EmailOrUserName);

        var user = await userRepository.FirstOrDefaultAsync(p =>
            EF.Functions.Collate(p.Email.Value, LoginNameMatcher.Collation) == identifier
            || EF.Functions.Collate(p.UserName.Value, LoginNameMatcher.Collation) == identifier,
            cancellationToken);

        if (user is null)
        {
            return Result<LoginCommandResponse>.Failure("Kullanıcı adı ya da şifre yanlış");
        }

        // VerifyPassword, eski biçimde saklanan hash'i (tek turlu HMAC-SHA512) algılar
        // ve parola doğruysa kendini PBKDF2'ye yükseltir. Böylece geçiş kademeli
        // olur: kullanıcılar şifrelerini değiştirmek zorunda kalmaz ve toplu bir
        // yeniden hashleme işlemine gerek kalmaz.
        var checkPassword = user.VerifyPassword(request.Password);

        if (checkPassword == PasswordVerification.Failed)
        {
            return Result<LoginCommandResponse>.Failure("Kullanıcı adı ya da şifre yanlış");
        }

        // Normal kullanıcılar yalnızca kendi kurumunda oturum açabilir; kurum
        // seçimi yalnızca sys_admin'e sunulur.
        if (request.CompanyId is { } selectedCompanyId && user.CompanyId.Value != selectedCompanyId)
        {
            var role = await roleRepository.FirstOrDefaultAsync(r => r.Id == user.RoleId, cancellationToken);

            if (role?.Name.Value != SystemRoles.SysAdmin)
            {
                return Result<LoginCommandResponse>.Failure(
                    "Bu kullanıcı seçilen kuruma ait değil. Kendi kurumunuzla giriş yapın.");
            }
        }

        var token = await jwtProvider.CreateTokenAsync(user, cancellationToken);
        return new LoginCommandResponse() { Token = token };
    }
}
