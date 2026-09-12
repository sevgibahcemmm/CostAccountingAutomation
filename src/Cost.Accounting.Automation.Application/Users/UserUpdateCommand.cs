using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Photos;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Domain.Users.ValueObjects;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Users;
[Permission("user:update")]
public sealed record UserUpdateCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string UserName,
    Guid? CompanyId,
    Guid RoleId,
    bool IsActive,
    string? TRIdentityNumber = null,
    List<PhotoInput>? Photos = null) : IRequest<Result<string>>;

public sealed class UserUpdateCommandValidator : AbstractValidator<UserUpdateCommand>
{
    public UserUpdateCommandValidator()
    {
        RuleFor(p => p.FirstName).NotEmpty().WithMessage("Geçerli bir ad girin");
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

internal sealed class UserUpdateCommandHandler(
    IUserRepository userRepository,
    IPhotoRepository photoRepository,
    IClaimContext claimContext) : IRequestHandler<UserUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UserUpdateCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (user is null)
        {
            return Result<string>.Failure("Kullanıcı bulunamadı");
        }

        if (user.Email.Value != request.Email)
        {
            var emailExists = await userRepository.AnyAsync(p => p.Email.Value == request.Email, cancellationToken);
            if (emailExists)
            {
                return Result<string>.Failure("Bu mail adresi daha önce kullanılmış");
            }
        }

        if (user.UserName.Value != request.UserName)
        {
            var userNameExists = await userRepository.AnyAsync(p => p.UserName.Value == request.UserName, cancellationToken);
            if (userNameExists)
            {
                return Result<string>.Failure("Bu kullanıcı adı daha önce kullanılmış");
            }
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
        IdentityId CompanyIdRecord = new(CompanyId);
        IdentityId roleId = new(request.RoleId);
        TRIdentityNumber? trIdentityNumber = string.IsNullOrWhiteSpace(request.TRIdentityNumber) ? null : new TRIdentityNumber(request.TRIdentityNumber);
        user.SetFirstName(firstName);
        user.SetLastName(lastName);
        user.SetFullName();
        user.SetEmail(email);
        user.SetUserName(userName);
        user.SetCompanyId(CompanyIdRecord);
        user.SetRoleId(roleId);
        user.SetStatus(request.IsActive);
        user.SetTRIdentityNumber(trIdentityNumber);
        userRepository.Update(user);

        List<Photo> existingPhotos = await photoRepository
            .Where(p => p.UserId == user.Id)
            .ToListAsync(cancellationToken);
        if (existingPhotos is { Count: > 0 })
        {
            photoRepository.SoftDeleteRange(existingPhotos);
        }

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

        return "Kullanıcı başarıyla güncellendi";
    }
}