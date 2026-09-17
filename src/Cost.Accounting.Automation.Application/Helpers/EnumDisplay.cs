using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Cost.Accounting.Automation.Application.Helpers;

public static class EnumDisplay
{
    public static string GetDisplayName<T>(T value) where T : struct, Enum
    {
        FieldInfo? field = typeof(T).GetField(value.ToString());
        if (field is null)
        {
            return value.ToString();
        }

        var attr = field.GetCustomAttributes(typeof(DisplayAttribute), false)
            .Cast<DisplayAttribute>()
            .FirstOrDefault();

        return attr is { Name: { Length: > 0 } } ? attr.Name : value.ToString();
    }

    public static string GetDisplayDescription<T>(T value) where T : struct, Enum
    {
        FieldInfo? field = typeof(T).GetField(value.ToString());
        if (field is null)
        {
            return string.Empty;
        }

        var attr = field.GetCustomAttributes(typeof(DisplayAttribute), false)
            .Cast<DisplayAttribute>()
            .FirstOrDefault();

        return attr?.Description ?? string.Empty;
    }
}