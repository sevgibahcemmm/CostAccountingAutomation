using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Photos;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.Users.ValueObjects;

namespace Cost.Accounting.Automation.Domain.Users;
public sealed class User : Entity
{
    public User(
        FirstName firstName,
        LastName lastName,
        Email email,
        UserName userName,
        Password password,
        IdentityId companyId,
        IdentityId roleId,
        bool isActive,
        TRIdentityNumber? tRIdentityNumber = null
        )
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetEmail(email);
        SetUserName(userName);
        SetPassword(password);
        SetFullName();
        SetIsForgotPasswordCompleted(new(true));
        SetCompanyId(companyId);
        SetRoleId(roleId);
        SetStatus(isActive);
        SetTRIdentityNumber(tRIdentityNumber);
    }

    private User() { }
    public FirstName FirstName { get; private set; } = default!;
    public LastName LastName { get; private set; } = default!;
    public FullName FullName { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public UserName UserName { get; private set; } = default!;
    public Password Password { get; private set; } = default!;
    public ForgotPasswordCode? ForgotPasswordCode { get; private set; }
    public ForgotPasswordDate? ForgotPasswordDate { get; private set; }
    public IsForgotPasswordCompleted IsForgotPasswordCompleted { get; private set; } = default!;
    public IdentityId CompanyId { get; private set; } = default!;
    public IdentityId RoleId { get; private set; } = default!;
    public TRIdentityNumber? TRIdentityNumber { get; private set; }
    public ICollection<Photo> Photos { get; private set; } = new List<Photo>();

    #region Behaviors
    public bool VerifyPasswordHash(string password)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA512(Password.PasswordSalt);
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(Password.PasswordHash);
    }

    public void CreateForgotPasswordId()
    {
        ForgotPasswordCode = new(Guid.CreateVersion7());
        ForgotPasswordDate = new(DateTimeOffset.Now);
        IsForgotPasswordCompleted = new(false);
    }

    public void SetFirstName(FirstName firstName)
    {
        FirstName = firstName;
    }

    public void SetLastName(LastName lastName)
    {
        LastName = lastName;
    }

    public void SetEmail(Email email)
    {
        Email = email;
    }

    public void SetUserName(UserName userName)
    {
        UserName = userName;
    }

    public void SetFullName()
    {
        FullName = new(FirstName.Value + " " + LastName.Value + " (" + Email.Value + ")");
    }

    public void SetPassword(Password password)
    {
        Password = password;
    }

    public void SetIsForgotPasswordCompleted(IsForgotPasswordCompleted isForgotPasswordCompleted)
    {
        IsForgotPasswordCompleted = isForgotPasswordCompleted;
    }

    public void SetCompanyId(IdentityId companyId)
    {
        CompanyId = companyId;
    }

    public void SetRoleId(IdentityId roleId)
    {
        RoleId = roleId;
    }

    public void SetTRIdentityNumber(TRIdentityNumber? tRIdentityNumber)
    {
        TRIdentityNumber = tRIdentityNumber;
    }
    #endregion
}