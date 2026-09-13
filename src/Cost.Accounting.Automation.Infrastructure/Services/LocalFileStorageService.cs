using Cost.Accounting.Automation.Application.Services;
using Microsoft.Extensions.Configuration;

namespace Cost.Accounting.Automation.Infrastructure.Services;

internal sealed class LocalFileStorageService : IFileStorageService
{
    private const string StorageFolderName = "wwwroot";

    public LocalFileStorageService(IConfiguration configuration)
    {
        string? configured = configuration["FileStorage:RootPath"];

        RootPath = string.IsNullOrWhiteSpace(configured)
            ? ResolveDefaultRoot()
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configured));

        Directory.CreateDirectory(RootPath);
    }

    public string RootPath { get; }

    public string GetFullPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return RootPath;

        string normalized = relativePath
            .TrimStart('/', '\\')
            .Replace('/', Path.DirectorySeparatorChar);

        return Path.Combine(RootPath, normalized);
    }

    public async Task<string> SaveAsync(
        byte[] content, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        if (content is null || content.Length == 0)
            throw new ArgumentException("Dosya içeriği boş olamaz.", nameof(content));

        string safeExtension = SanitizeExtension(Path.GetExtension(fileName));
        string uniqueFileName = $"{Guid.NewGuid():N}{safeExtension}";

        string targetDirectory = Path.Combine(RootPath, folder);
        Directory.CreateDirectory(targetDirectory);

        string fullPath = Path.Combine(targetDirectory, uniqueFileName);
        await File.WriteAllBytesAsync(fullPath, content, cancellationToken);

        return $"{folder}\\{uniqueFileName}";
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return Task.CompletedTask;

        string fullPath = GetFullPath(relativePath);

        try
        {
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch
        {
            // Silme sırasında oluşan hatalar uygulamayı durdurmamalı
        }

        return Task.CompletedTask;
    }

    private static string ResolveDefaultRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        for (int depth = 0; depth < 8 && directory is not null; depth++)
        {
            string candidate = Path.Combine(directory.FullName, StorageFolderName);
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }

    private static string SanitizeExtension(string extension)
    {
        string[] allowed = [".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp"];
        string ext = extension?.ToLowerInvariant() ?? string.Empty;
        return allowed.Contains(ext) ? ext : ".png";
    }
}
