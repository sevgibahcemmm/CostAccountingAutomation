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
    IFileStorageService fileStorage,
    IClaimContext claimContext,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<UserCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UserCreateCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await userRepository.AnyAsync(p => p.Email.Value == request.Email, cancellationToken);
        if (emailExists)
        {
            return Result<string>.Failure("Bu mail adresi daha önce kullanılmış");
        }

        string? duplicateKey = User.BuildDuplicateKey(request.UserName);

        User? userNameDuplicate = await duplicateCheckService.FindMasterDuplicateAsync<User>(
            duplicateKey,
            cancellationToken: cancellationToken);

        if (userNameDuplicate is not null)
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
            var avatar = ResolveAvatar(request.Photos);

            string relativePath = await fileStorage.SaveAsync(
                avatar.Data, avatar.FileName, "UserImages", cancellationToken);

            user.SetAvatarPath(relativePath);
        }

        return Result<string>.Succeed("Kullanıcı başarıyla oluşturuldu");
    }

    /// <summary>
    /// Kullanıcı verileri master veritabanında tek bir avatar yoluna sahip
    /// olabildiği için çoklu fotoğraf listesinden varsayılan (yoksa ilk) seçilir.
    /// </summary>
    internal static PhotoInput ResolveAvatar(IReadOnlyList<PhotoInput> photos)
        => photos.FirstOrDefault(p => p.IsDefault) ?? photos[0];
}