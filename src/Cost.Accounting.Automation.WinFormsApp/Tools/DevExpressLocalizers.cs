using System.Globalization;
using System.IO;
using System.Reflection;

namespace Cost.Accounting.Automation.WinFormsApp.Tools;

/// <summary>
/// DevExpress arayüzünü Türkçeleştirmek için kullanılan yardımcı sınıf.
/// Uygulamanın kültürünü tr-TR yapar ve çıktı dizinindeki "tr"/"tr-TR"
/// klasörlerindeki uydu kaynak derlemelerini (.resources.dll) belleğe yükler.
/// </summary>
public static class DevExpressLocalizers
{
    private const string TargetCultureName = "tr-TR";

    private static readonly object SyncLock = new();
    private static readonly List<Assembly> LoadedSatelliteAssembliesList = [];

    /// <summary>
    /// DevExpress kontrollerinin Türkçe açılması için gereken tüm kurulumu yapar.
    /// Uygulamanın <c>Main()</c> metodunun en başında, herhangi bir form veya kontrol
    /// oluşturulmadan önce çağrılmalıdır.
    /// </summary>
    public static void Register()
    {
        lock (SyncLock)
        {
            ApplyCulture();
            LoadSatelliteAssemblies();
        }
    }

    private static void ApplyCulture()
    {
        var culture = CultureInfo.GetCultureInfo(TargetCultureName);

        // Mevcut iş parçacığı ve sonradan açılacak tüm yeni iş parçacıkları için kültürü ayarlar.
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    private static void LoadSatelliteAssemblies()
    {
        var appDirectory = AppContext.BaseDirectory;

        // tr-TR hem "tr-TR" hem "tr" klasöründe aranır.
        var candidateFolders = new[]
        {
            Path.Combine(appDirectory, "tr-TR"),
            Path.Combine(appDirectory, "tr")
        };

        foreach (var folder in candidateFolders)
        {
            if (!Directory.Exists(folder))
                continue;

            foreach (var filePath in Directory.EnumerateFiles(folder, "*.resources.dll", SearchOption.TopDirectoryOnly))
            {
                LoadAssembly(filePath);
            }
        }
    }

    private static void LoadAssembly(string filePath)
    {
        try
        {
            var assembly = Assembly.LoadFrom(filePath);

            if (!LoadedSatelliteAssembliesList.Contains(assembly))
            {
                LoadedSatelliteAssembliesList.Add(assembly);
            }
        }
        catch
        {
            // Bozuk veya uyumsuz sürümdeki uydu derlemeleri yoksayılır.
        }
    }
}