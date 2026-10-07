using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Users;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Behaviors;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class PermissionAttribute : Attribute
{
    public string? Permission { get; }
    public PermissionAttribute() { }
    public PermissionAttribute(string permission) => Permission = permission;
}

public sealed class PermissionBehavior<TRequest, TResponse>(
    IServiceScopeFactory _scopeFactory,
    IClaimContext _userContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        var attr = request.GetType().GetCustomAttribute<PermissionAttribute>(inherit: true);
        if (attr is null) return await next();

        var userId = _userContext.GetUserId();

        if (userId == Guid.Empty) return await next();

        RoleAccess access = await RoleAccessCache.GetAsync(_scopeFactory, userId, cancellationToken);

        if (access.RoleName == "sys_admin") return await next();

        if (!string.IsNullOrEmpty(attr.Permission))
        {
            if (!access.HasPermission(attr.Permission))
                throw new AuthorizationException($"'{attr.Permission}' yetkisine sahip değilsiniz.");
        }
        else if (access.RoleName != "Admin")
        {
            throw new AuthorizationException("Bu işlem için admin yetkisi gereklidir.");
        }

        return await next();
    }
}

public sealed class AuthorizationException : Exception
{
    public AuthorizationException() : base("Yetkiniz bulunmamaktadır.") { }
    public AuthorizationException(string message) : base(message) { }
}

/// <summary>
/// Bir kullanıcının yetki denetimi için gereken, önbelleklenmiş rol bilgisi.
/// </summary>
internal sealed record RoleAccess(Guid RoleId, string RoleName, HashSet<string> Permissions)
{
    /// <summary>Eşitlik, davranış değişmediği için ordinal (büyük/küçük harf duyarlı) tutulur.</summary>
    public bool HasPermission(string permission) => Permissions.Contains(permission);
}

/// <summary>
/// <see cref="PermissionBehavior{TRequest,TResponse}"/>'ın kullanıcı+rol
/// sorgularını istek başına tekrar okumasını engeller.
///
/// <para>
/// Eskiden her yetkili istekte Users ve Roles tablolarından iki ayrı sorgu
/// çalışıyordu; arayüz bir sayfa açarken onlarca kez tekrarlanan bu sorgular
/// hem veritabanı hem de tanı logu (SQL interceptor) yükü oluşturuyordu.
/// </para>
/// <para>
/// Önbellek oturum içindedir ve sınırlı bir ömre sahiptir (<see cref="Ttl"/>):
/// başka bir süreçte bir rol değiştirildiğinde en fazla bu kadar süre sonra
/// geçerli hâle gelir. Aynı süreçte rol yetkileri değiştiğinde
/// <see cref="InvalidateRole"/> ile anında düşürülür. Güvenliğin kaynağı
/// yine sunucu tarafı denetimdir; önbellek yalnızca aynı süreçteki kısa
/// tekrarları ortadan kaldırır.
/// </para>
/// </summary>
/// <remarks>
/// Sınıf bilinçli olarak generik değildir. <c>PermissionBehavior</c> her
/// istek tipi için ayrı bir kapalı generik tip olduğundan, önbelleğin o
/// tipin statik alanına konması her istek için ayrı önbellek anlamına
/// gelirdi.
/// </remarks>
internal static class RoleAccessCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

    private static readonly ConcurrentDictionary<Guid, (RoleAccess Access, DateTimeOffset ExpiresAt)> Cache = new();

    public static async Task<RoleAccess> GetAsync(
        IServiceScopeFactory scopeFactory,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (Cache.TryGetValue(userId, out (RoleAccess Access, DateTimeOffset ExpiresAt) entry)
            && entry.ExpiresAt > DateTimeOffset.UtcNow)
        {
            return entry.Access;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var user = await userRepository.FirstOrDefaultAsync(p => p.Id == userId, cancellationToken);
        if (user is null) throw new AuthorizationException("Kullanıcı bulunamadı.");

        var role = await roleRepository.FirstOrDefaultAsync(r => r.Id == user.RoleId, cancellationToken);
        if (role is null) throw new AuthorizationException("Kullanıcıya atanmış geçerli bir rol bulunamadı.");

        var access = new RoleAccess(
            role.Id,
            role.Name.Value,
            role.Permissions.Select(p => p.Value).ToHashSet(StringComparer.Ordinal));

        Cache[userId] = (access, DateTimeOffset.UtcNow.Add(Ttl));
        return access;
    }

    /// <summary>Verilen rola ait tüm önbellek girişlerini düşürür.</summary>
    public static void InvalidateRole(Guid roleId)
    {
        foreach (KeyValuePair<Guid, (RoleAccess Access, DateTimeOffset ExpiresAt)> entry in Cache)
        {
            if (entry.Value.Access.RoleId == roleId)
            {
                Cache.TryRemove(entry.Key, out _);
            }
        }
    }
}
