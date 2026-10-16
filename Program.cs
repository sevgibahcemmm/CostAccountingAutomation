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
}
}
    /// <summary>
    /// Raporun gerçekten sayfa ürettiğini ve sekmenin yazdırma düğmelerinin
    /// AYNI belgeyi gösterdiğini doğrular.
    ///
    /// <para>
    /// Doğrulama görüntüleyicinin <c>Document</c> özelliği üzerinden değil,
    /// yazdırma sistemi üzerinden yapılır: DevExpress belge yerleşimini ancak
    /// pencere bir handle kazandığında tamamlar, dolayısıyla gösterilmeyen bir
    /// sekmede <c>viewer.Document</c> boş görünür — bu bir hata değildir.
    /// Gerçek risk şudur: izleyici ile yazdırma aracı FARKLI belgeler
    /// taşıyabilir; o zaman ekranda görünen ile kâğıda çıkan farklılaşır.
    /// </para>
    /// </summary>
    private static void VerifyDocumentRendering(ref int failures)
    {
        Console.WriteLine();
        Console.WriteLine("--- Belge uretimi ---");

        try
        {
            // Projede gercekten kullanilan bir rapor sinifi: sablonu, bantlari
            // ve stil dosyasiyla birlikte yuklenir.
            using StockMovementsListReport report = new();

            // Uretim akisi: belge ONCE bekleyece penceresiyle uretilir, sonra
            // sekme acilir. Burada da ayni sirayi izliyoruz.
            report.CreateDocument();

            ReportViewerMdiForm form = new(report, "BelgeTesti");

            Invoke(form, "OnLoad", () => typeof(ReportViewerMdiForm)
                .GetMethod("OnLoad", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(form, [new EventArgs()]));

            DocumentViewer? viewer = FindViewer(form);

            if (viewer is null)
            {
                Fail("belge izleyici bulunamadi", ref failures);
                return;
            }

            int pages = viewer.PrintingSystem.Document?.Pages.Count ?? 0;

            Console.WriteLine($"  yazdirma sistemi sayfasi     : {pages}");

            if (pages <= 0)
            {
                Fail("belge uretilmedi (0 sayfa)", ref failures);
            }
            else
            {
                Console.WriteLine("  belge uretildi              : OK");
            }

            // Izleyici ile yazdirma araci ayni yazdirma sistemini paylasmali;
            // aksi halde ekrandaki ile yazdirilan farklilasir.
            object? tool = typeof(ReportViewerMdiForm)
                .GetField("_printTool", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(form);

            var toolSystem = (tool as DevExpress.XtraReports.UI.ReportPrintTool)?.PrintingSystem;

            bool shared = ReferenceEquals(toolSystem, viewer.PrintingSystem);

            Console.WriteLine($"  izleyici/yazdirma ayni mi   : {(shared ? "EVET" : "HAYIR")}");

            if (!shared)
            {
                Fail("izleyici ve yazdirma araci farkli belgeleri gosteriyor", ref failures);
            }

            form.Dispose();
        }
        catch (Exception ex)
        {
            Fail("belge uretilemedi: " + ex.Message, ref failures);
        }
    }

}
