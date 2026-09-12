using System.Reflection;
using Cost.Accounting.Automation.Application.Behaviors;

namespace Cost.Accounting.Automation.Application.Services;
public sealed class PermissionService
{
    public List<string> GetAll()
    {
        var permissions = new HashSet<string>();
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
}
