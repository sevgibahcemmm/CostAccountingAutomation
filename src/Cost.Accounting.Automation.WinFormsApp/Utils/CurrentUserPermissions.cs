using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Application.Users;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Utils;

/// <summary>
/// Oturumu açan kullanıcının yetkilerini arayüzde kullanılabilir hâle getirir.
/// </summary>
/// <remarks>
/// <para>
/// Amaç, kullanıcının yetkisi olmayan düğmeleri hiç göstermemektir; örneğin
/// duyuru gönderme düğmesi yalnızca <c>message:announce</c> yetkisi olanlarda
/// görünür.
/// </para>
/// <para>
/// Güvenlik buradan gelmez. Güvenlik her zaman sunucu tarafındaki
/// <c>PermissionBehavior</c> ile sağlanır; düğme gizlense bile doğrudan
/// komut gönderilirse komut reddedilir. Buradaki önbellek yalnızca kullanıcı
/// deneyimidir.
/// </para>
/// </remarks>
public static class CurrentUserPermissions
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private static UserPermissionsDto? _cache;

    /// <summary>
    /// Oturumu açan kullanıcının yetkilerini döner. Sonuç oturum boyunca
    /// bir kez okunup saklanır; kullanıcı değişirse (yeni giriş) önbellek
    /// geçersizleşir.
    /// </summary>
    public static async Task<UserPermissionsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        Guid currentUserId = SessionClaimContextProbe.CurrentUserIdOrDefault() ?? Guid.Empty;

        UserPermissionsDto? cached = _cache;

        if (cached is not null && (currentUserId == Guid.Empty || cached.UserId == currentUserId))
        {
            return cached;
        }

        await Gate.WaitAsync(cancellationToken);

        try
        {
            cached = _cache;

            if (cached is not null && (currentUserId == Guid.Empty || cached.UserId == currentUserId))
            {
                return cached;
            }

            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            Result<UserPermissionsDto> result = await mediator.Send(
                new CurrentUserPermissionsQuery(), cancellationToken);

            if (!result.IsSuccessful || result.Data is null)
            {
                // Yetkiler okunamazsa arayüz kısıtlaması uygulanamaz; bu
                // durumda düğmeleri gizlemek yerine sunucudaki reddi göstermek
                // daha doğrudur, bu yüzden "hepsine izin ver" varsayılır.
                _cache = new UserPermissionsDto { IsSysAdmin = true };
                return _cache;
            }

            _cache = result.Data;
            return _cache;
        }
        catch (Exception ex)
        {
            CrashLog.WriteException("CurrentUserPermissions", ex);

            _cache = new UserPermissionsDto { IsSysAdmin = true };
            return _cache;
        }
        finally
        {
            Gate.Release();
        }
    }

    /// <summary>Verilen yetkiye sahip olup olmadığını döner.</summary>
    public static async Task<bool> HasAsync(string permission, CancellationToken cancellationToken = default)
    {
        UserPermissionsDto permissions = await GetAsync(cancellationToken);

        return permissions.Has(permission);
    }

    /// <summary>Çıkışta veya rol değişiminde önbelleği düşürür.</summary>
    public static void Invalidate() => _cache = null;
}

/// <summary>
/// <see cref="IClaimContext"/> bir singleton olarak kayıtlı olduğu için
/// oturumdaki kullanıcı kimliğine buradan erişilir. Probe sınıfı, arayüzün
/// somut <c>SessionClaimContext</c> tipine bağımlı kalmaması için vardır.
/// </summary>
internal static class SessionClaimContextProbe
{
    public static Guid? CurrentUserIdOrDefault()
    {
        try
        {
            IClaimContext? claims = Program.Services.GetService<IClaimContext>();

            return claims?.GetUserIdOrDefault();
        }
        catch
        {
            return null;
        }
    }
}