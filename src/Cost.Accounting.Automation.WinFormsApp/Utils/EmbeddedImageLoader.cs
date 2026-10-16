using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Cost.Accounting.Automation.WinFormsApp.Utils;

/// <summary>
/// Projeye gömülü (EmbeddedResource) görselleri yükler. Kaynak sistemin
/// (<c>C:\AMPLTD\AMPDT</c>) DFM dosyalarından çıkarılan PNG'ler bu yolla okunur.
/// </summary>
internal static class EmbeddedImageLoader
{
    /// <summary>
    /// Verilen dosya adıyla biten gömülü kaynağı bulup bağımsız bir
    /// <see cref="Bitmap"/> olarak döndürür. Kaynak yoksa <see langword="null"/>.
    /// </summary>
    public static Image? Load(string fileName)
    {
        Assembly assembly = typeof(EmbeddedImageLoader).Assembly;

        string? name = Array.Find(
            assembly.GetManifestResourceNames(),
            n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

        if (name is null)
        {
            return null;
        }

        using Stream? stream = assembly.GetManifestResourceStream(name);
        if (stream is null)
        {
            return null;
        }

        using Image loaded = Image.FromStream(stream);

        // Stream kapanacağı için görseli bağımsız bir bitmap'e kopyalarız.
        return new Bitmap(loaded);
    }
}
