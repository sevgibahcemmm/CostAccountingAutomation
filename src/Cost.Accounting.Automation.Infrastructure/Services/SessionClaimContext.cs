using Cost.Accounting.Automation.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cost.Accounting.Automation.Infrastructure.Services;


/// <summary>
/// WinForms oturumu boyunca giriş yapan kullanıcı bilgisini tutar.
/// Login formu başarılı girişten sonra SetCurrentUser çağırır.
/// </summary>
public sealed class SessionClaimContext : IClaimContext
{
    private Guid? _userId;
    private Guid? _companyId;
    private string? _roleName;
    private string? _userFullName;

    public string? Token { get; private set; }

    public void SetCurrentUser(Guid userId, Guid companyId, string roleName, string? userFullName, string? token = null)
    {
        _userId = userId;
        _companyId = companyId;
        _roleName = roleName;
        _userFullName = userFullName;
        Token = token;
    }

    public void Clear()
    {
        _userId = null;
        _companyId = null;
        _roleName = null;
        _userFullName = null;
    }

    public Guid GetUserId()
        => _userId ?? throw new InvalidOperationException("Oturum açık değil: kullanıcı bilgisi bulunamadı");

    public Guid GetCompanyId()
        => _companyId ?? throw new InvalidOperationException("Oturum açık değil: şube bilgisi bulunamadı");

    public string GetRoleName()
        => _roleName ?? throw new InvalidOperationException("Oturum açık değil: rol bilgisi bulunamadı");

    public string? GetUserFullName()
        => _userFullName ?? throw new InvalidOperationException("Oturum açık değil: kullanıcı adı bilgisi bulunamadı");

    public Guid? GetUserIdOrDefault() => _userId;
}