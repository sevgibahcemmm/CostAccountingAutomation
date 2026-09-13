using System.IO;
using Cost.Accounting.Automation.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cost.Accounting.Automation.WinFormsApp.Tools;

internal static class StorageRoot
{
    private static readonly Lazy<IFileStorageService> _storage = new(() =>
    {
        using IServiceScope scope = Program.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IFileStorageService>();
    });

    internal static string Root
    {
        get
        {
            Directory.CreateDirectory(_storage.Value.RootPath);
            return _storage.Value.RootPath;
        }
    }

    internal static string Resolve(string relativePath)
        => _storage.Value.GetFullPath(relativePath);
}