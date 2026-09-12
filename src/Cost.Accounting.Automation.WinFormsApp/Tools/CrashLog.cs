using System.IO;

namespace Cost.Accounting.Automation.WinFormsApp.Tools;

/// <summary>
/// Çökme ve kritik adımları exe yanındaki logs\crash.log dosyasına yazar.
/// Geçici tanılama aracı; sorun çözüldükten sonra kaldırılabilir.
/// </summary>
internal static class CrashLog
{
    public static void Write(string category, string message)
    {
        try
        {
            string dir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(dir);
            File.AppendAllText(
                Path.Combine(dir, "crash.log"),
                $"[{DateTimeOffset.Now:O}] [{category}] {message}\n");
        }
        catch
        {
        }
    }

    public static void WriteException(string category, Exception exception)
    {
        Write(category, exception.ToString());
    }
}