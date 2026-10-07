using System.Reflection;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Messages;
using Cost.Accounting.Automation.Domain.Roles;
using GenericRepository;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Application.Services;
public sealed class PermissionService
{
    public List<string> GetAll()
    {
        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        permissions.Add("dashboard:view");

        var assembly = Assembly.GetExecutingAssembly();

        IReadOnlyList<Type> types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(t => t is not null).ToList()!;
        }

        foreach (var type in types)
        {
            var permissinAttr = type.GetCustomAttribute<PermissionAttribute>();

            if (permissinAttr is not null && !string.IsNullOrEmpty(permissinAttr.Permission))
            {
                permissions.Add(permissinAttr.Permission);
            }
        }

        return permissions.ToList();
    }

    /// <summary>
    /// Rollerin **başlangıç** yetkileri.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>sys_admin</c> dışındaki roller boş doğar. Bu doğru bir tercihtir: yeni
    /// bir yetki tanımlanınca otomatik olarak herkese açılmamalıdır. Ama mesajlaşma
    /// gibi temel bir özelliğin yalnızca yöneticide çalışması da kabul edilemez;
    /// o durumda özellik kullanılamaz hale gelir.
    /// </para>
    /// <para>
    /// Bu yüzden yalnızca <b>herkesin ihtiyaç duyduğu</b> yetkiler burada
    /// listelenir. Geri kalan her şey yönetici tarafından rol ekranından verilir.
    /// </para>
    /// </remarks>
    private static readonly Dictionary<string, string[]> StarterPermissions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Mesajlaşma: kullanıcılar kendi aralarında yazabilir ve okuyabilir.
            // Duyuru bu yetkilere dâhil DEĞİLDİR: duyuru herkesin kutusuna
            // düştüğü için yalnızca yöneticiye aittir (bkz.
            // EnsureAnnouncementOnlyForAdminAsync).
            ["muhasebe_muduru"] = ["message:view", "message:send"],
            ["muhasebe_elemani"] = ["message:view", "message:send"],
        };

    /// <summary>
    /// Standart rollerin başlangıç yetkilerini eksik olanlar için tamamlar.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Yalnızca ekler, hiçbir şeyi kaldırmaz.</b> Yöneticinin rol ekranından
    /// verdiği ek yetkiler ve kaldırdığı bir yetki korunur; yalnızca rolün
    /// sahip olmadığı başlangıç yetkileri tamamlanır. Bu yüzden yönetici
    /// bir kullanıcının mesajlaşmasını kapatmak istiyorsa yetkiyi listeden
    /// kaldırması yeterlidir; sonraki açılışta geri gelmez.
    /// </para>
    /// <para>
    /// Operasyon idempotenttir ve yalnızca eksik olanı bulduğunda yazar.
    /// </para>
    /// </remarks>
    public async Task EnsureStarterRolePermissionsAsync(
        IRoleRepository roleRepository,
        IMasterUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        if (StarterPermissions.Count == 0)
        {
            return;
        }

        var changed = new List<Role>();

        // Roller tek tek FirstOrDefaultAsync ile cekilir. Where(...).ToListAsync
        // kullanmak izlememe (tracking) davranisi degisebilir; sahip olunan
        // Permission koleksiyonu izlenmiyorse yeni izin kaydi kaydedilemez
        // ("shadow key property Permission.Id is unknown").
        foreach (string roleName in StarterPermissions.Keys)
        {
            Role? role = await roleRepository
                .FirstOrDefaultAsync(r => r.Name.Value == roleName, cancellationToken);

            if (role is null)
            {
                continue;
            }

            if (!StarterPermissions.TryGetValue(role.Name.Value, out string[]? starters))
            {
                continue;
            }

            var merged = new HashSet<string>(
                role.Permissions.Select(p => p.Value),
                StringComparer.OrdinalIgnoreCase);

            bool missing = starters.Any(p => !merged.Contains(p));

            if (!missing)
            {
                continue;
            }

            foreach (string permission in starters)
            {
                merged.Add(permission);
            }

            role.SetPermissions(merged.Select(p => new Permission(p)));
            roleRepository.Update(role);
            changed.Add(role);
        }

        if (changed.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Duyuru yetkisinin (<c>message:announce</c>) yalnızca <c>sys_admin</c>
    /// rolünde kalmasını sağlar.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Duyuru, seçilen her kullanıcının kutusuna ayrı satır düşüren ve
    /// tüm kurumu ilgilendiren daha güçlü bir işlemdir; bu yüzden yetkisi
    /// bilinçli olarak yönetime ayrılmıştır. Başlangıç listesinden
    /// çıkarılması yalnızca yeni kurulumlar için yeterlidir: daha önce bu
    /// yetkiyi almış roller veritabanında durur ve yetkileri değişmeden
    /// kalırdı.
    /// </para>
    /// <para>
    /// Bu yüzden her açılışta, kaynak rol ekranından gelmiş olsa bile bu
    /// yetki yönetici dışındaki rollerden alınır. Sonuç öngörülebilirdir:
    /// duyuru her zaman yalnızca yönetici tarafından gönderilir.
    /// </para>
    /// </remarks>
    public async Task EnsureAnnouncementOnlyForAdminAsync(
        IRoleRepository roleRepository,
        IMasterUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        // Roller tek tek ve izlenerek çekilir; bkz.
        // EnsureStarterRolePermissionsAsync açıklaması (izlenmeyen koleksiyon
        // üzerindeki çıkarma kaydedilemez).
        List<string> roleNames = await roleRepository
            .Where(r => r.Name.Value != "sys_admin")
            .Select(r => r.Name.Value)
            .ToListAsync(cancellationToken);

        var changed = new List<Role>();

        foreach (string roleName in roleNames)
        {
            Role? role = await roleRepository
                .FirstOrDefaultAsync(r => r.Name.Value == roleName, cancellationToken);

            if (role is null)
            {
                continue;
            }

            bool hasAnnounce = role.Permissions.Any(p =>
                string.Equals(p.Value, MessagePermissions.Announce, StringComparison.OrdinalIgnoreCase));

            if (!hasAnnounce)
            {
                continue;
            }

            role.SetPermissions(role.Permissions
                .Where(p => !string.Equals(
                    p.Value, MessagePermissions.Announce, StringComparison.OrdinalIgnoreCase))
                .Select(p => new Permission(p.Value)));

            roleRepository.Update(role);
            changed.Add(role);
        }

        if (changed.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Command/query sınıflarındaki tüm [Permission] bildirimlerini tarayıp
    /// sys_admin rolüne otomatik olarak ekler. Yeni bir yetki tanımlandığında
    /// elle müdahale gerekmeden bir sonraki başlangıçta devreye girer.
    /// </summary>
    public async Task EnsureAdminRoleHasAllPermissionsAsync(
        IRoleRepository roleRepository,
        IMasterUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        List<string> catalog = GetAll();

        Role? adminRole = await roleRepository.FirstOrDefaultAsync(
            r => r.Name.Value == "sys_admin",
            cancellationToken);

        if (adminRole is null)
        {
            return;
        }

        bool anyMissing = catalog.Any(p => !adminRole.Permissions.Any(
            x => string.Equals(x.Value, p, StringComparison.OrdinalIgnoreCase)));

        if (!anyMissing)
        {
            return;
        }

        var merged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string permission in adminRole.Permissions.Select(x => x.Value))
        {
            merged.Add(permission);
        }
        foreach (string permission in catalog)
        {
            merged.Add(permission);
        }

        adminRole.SetPermissions(merged.Select(p => new Permission(p)));
        roleRepository.Update(adminRole);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}