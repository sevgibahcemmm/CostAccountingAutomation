using System.Reflection;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Roles;
using GenericRepository;

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