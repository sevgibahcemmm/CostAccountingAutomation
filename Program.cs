using System;
using System.Linq;
using System.Reflection;

internal static class P
{
    private static void Main()
    {
        var g = Assembly.LoadFrom(@"C:\Users\e_akp\.nuget\packages\devexpress.win.navigation\25.2.3\lib\net8.0-windows\DevExpress.XtraEditors.v25.2.dll");
        Type[] ts;
        try { ts = g.GetTypes(); }
        catch (ReflectionTypeLoadException e) { ts = e.Types.Where(t => t != null).ToArray(); }

        var t = ts.FirstOrDefault(x => x.Name == "ToggleSwitch");
        Console.WriteLine("ToggleSwitch -> " + (t?.FullName ?? "BULUNAMADI"));
        if (t == null)
        {
            foreach (var n in ts.Where(x => x.Name.Contains("Toggle")).Take(20)) Console.WriteLine("  " + n.FullName);
            return;
        }

        Console.WriteLine("  base: " + t.BaseType.FullName);
        Console.WriteLine("--- properties ---");
        foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(p => p.Name))
            Console.WriteLine("  " + p.PropertyType.Name + " " + p.Name + (p.CanWrite ? " {get;set;}" : " {get;}"));
        Console.WriteLine("--- events ---");
        foreach (var e in t.GetEvents(BindingFlags.Public | BindingFlags.Instance).OrderBy(e => e.Name))
            Console.WriteLine("  " + e.EventHandlerType.Name + " " + e.Name);

        var pt = ts.FirstOrDefault(x => x.Name == "ToggleSwitchProperties");
        if (pt != null)
        {
            Console.WriteLine("--- ToggleSwitchProperties ---");
            foreach (var p in pt.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(p => p.Name))
                Console.WriteLine("  " + p.PropertyType.Name + " " + p.Name + (p.CanWrite ? " {get;set;}" : " {get;}"));
        }
    }
}
