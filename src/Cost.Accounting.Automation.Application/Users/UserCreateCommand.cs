using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Photos;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Domain.Users.ValueObjects;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Users;
[Permission("user:create")]
public sealed record UserCreateCommand(
    string FirstName,
    string LastName,
    string Email,
    string UserName,
    Guid? CompanyId,
    Guid RoleId,
    bool IsActive,
    string? TRIdentityNumber = null,
    List<PhotoInput>? Photos = null) : IRequest<Result<string>>;

public sealed class UserCreateCommandValidator : AbstractValidator<UserCreateCommand>
{
    public UserCreateCommandValidator()
    {
        RuleFor(p => p.FirstName).NotEmpty().WithMessage("Geçerli bir ad girin.........................");
        RuleFor(p => p.LastName).NotEmpty().WithMessage("Geçerli bir soyad girin");
        RuleFor(p => p.UserName).NotEmpty().WithMessage("Geçerli bir kullanıcı adı girin");
        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("Geçerli bir mail adresi girin")
            .EmailAddress().WithMessage("Geçerli bir mail adresi girin");
        RuleFor(p => p.RoleId).NotEmpty().WithMessage("Rol seçin");
        RuleFor(p => p.TRIdentityNumber)
            .Matches("^[0-9]{11}$").WithMessage("Geçerli bir TC Kimlik No girin")
            .When(p => !string.IsNullOrWhiteSpace(p.TRIdentityNumber));
    }
}

internal sealed class UserCreateCommandHandler(
    IUserRepository userRepository,
    IPhotoRepository photoRepository,
    IClaimContext claimContext) : IRequestHandler<UserCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UserCreateCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await userRepository.AnyAsync(p => p.Email.Value == request.Email, cancellationToken);
        if (emailExists)
        {
            return Result<string>.Failure("Bu mail adresi daha önce kullanılmış");
        }

        var userNameExists = await userRepository.AnyAsync(p => p.UserName.Value == request.UserName, cancellationToken);
        if (userNameExists)
        {
            return Result<string>.Failure("Bu kullanıcı adı daha önce kullanılmış");
        }
        var tRIdentityNumberExists = await userRepository.AnyAsync(
            p => p.TRIdentityNumber != null && p.TRIdentityNumber.Value == request.TRIdentityNumber, cancellationToken);
        if (tRIdentityNumberExists)
        {
            return Result<string>.Failure("Bu kullanıcı TC numarası daha önce kullanılmış");
        }


        var CompanyId = claimContext.GetCompanyId();
        if (request.CompanyId is not null)
        {
            CompanyId = request.CompanyId.Value;
        }
        FirstName firstName = new(request.FirstName);
        LastName lastName = new(request.LastName);
        Email email = new(request.Email);
        UserName userName = new(request.UserName);
        Password password = new("123");
        IdentityId CompanyIdRecord = new(CompanyId);
        IdentityId roleId = new(request.RoleId);
        TRIdentityNumber? trIdentityNumber = string.IsNullOrWhiteSpace(request.TRIdentityNumber) ? null : new TRIdentityNumber(request.TRIdentityNumber);
        User user = new(
            firstName,
            lastName,
            email,
            userName,
            password,
            CompanyIdRecord,
            roleId,
            request.IsActive,
            trIdentityNumber);
        userRepository.Add(user);

        if (request.Photos is { Count: > 0 })
        {
            bool anyDefault = request.Photos.Any(p => p.IsDefault);
            for (int i = 0; i < request.Photos.Count; i++)
            {
                var photo = request.Photos[i];
                bool isDefault = photo.IsDefault || (!anyDefault && i == 0);
                photoRepository.Add(new Photo(user.Id, photo.FileName, photo.ContentType, photo.Data, isDefault));
            }
        }

        return Result<string>.Succeed("Kullanıcı başarıyla oluşturuldu");
    }
}