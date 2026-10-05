using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Users;

/// <summary>
/// Oturumu açan kullanıcının yetkilerini döner.
/// </summary>
/// <remarks>
/// <para>
/// Bu sorgunun kendisinde <c>[Permission]</c> özniteliği YOKTUR: kullanıcı
/// kendi yetkilerini okuyabilmelidir. Amaç, arayüzün yetkisiz olduğu
/// düğmeleri hiç göstermemesidir; güvenlik yine
/// <see cref="Behaviors.PermissionBehavior"/> tarafından uygulanır, yani
/// düğme gizlenmiş olsa bile doğrudan komut gönderilmesi reddedilir.
/// </para>
/// </remarks>
public sealed record CurrentUserPermissionsQuery : IRequest<Result<UserPermissionsDto>>;

/// <summary>Oturumu açan kullanıcının yetki özeti.</summary>
public sealed class UserPermissionsDto
{
    public Guid UserId { get; set; }

    public string UserFullName { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Sistem yöneticisi rol adı <c>sys_admin</c>'dir ve tüm yetkilere sahiptir;
    /// yetki listesi onun için boş döner.
    /// </summary>
    public bool IsSysAdmin { get; set; }

    public List<string> Permissions { get; set; } = [];

    /// <summary>Verilen yetkiye sahip olup olmadığı.</summary>
    public bool Has(string permission)
    {
        if (IsSysAdmin || string.IsNullOrWhiteSpace(permission))
        {
            return true;
        }

        return Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}

internal sealed class CurrentUserPermissionsQueryHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IClaimContext claimContext) : IRequestHandler<CurrentUserPermissionsQuery, Result<UserPermissionsDto>>
{
    /// <summary>Tüm yetkileri otomatik olarak taşıyan rol adı.</summary>
    private const string SysAdminRoleName = "sys_admin";

    public async Task<Result<UserPermissionsDto>> Handle(
        CurrentUserPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var me = new IdentityId(claimContext.GetUserId());

        var user = await userRepository.FirstOrDefaultAsync(p => p.Id == me, cancellationToken);

        if (user is null)
        {
            return Result<UserPermissionsDto>.Failure("Kullanıcı bulunamadı");
        }

        var role = await roleRepository.FirstOrDefaultAsync(
            r => r.Id == user.RoleId, cancellationToken);

        if (role is null)
        {
            return Result<UserPermissionsDto>.Failure("Kullanıcıya atanmış geçerli bir rol bulunamadı");
        }

        bool isSysAdmin = string.Equals(
            role.Name.Value, SysAdminRoleName, StringComparison.OrdinalIgnoreCase);

        return new UserPermissionsDto
        {
            UserId = me.Value,
            UserFullName = user.FullName.Value,
            RoleName = role.Name.Value,
            IsSysAdmin = isSysAdmin,
            Permissions = role.Permissions
                .Select(p => p.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .OrderBy(v => v, StringComparer.Ordinal)
                .ToList()
        };
    }
}