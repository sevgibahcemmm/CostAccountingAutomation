using Cost.Accounting.Automation.Application.Updates;
using Cost.Accounting.Automation.Domain.Updates;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Services;

internal sealed class AppReleaseService(MasterDbContext masterDbContext) : IAppReleaseService
{
    public async Task<AppReleaseInfo?> GetLatestPublishedAsync(CancellationToken cancellationToken = default)
    {
        return await masterDbContext.AppReleases
            .AsNoTracking()
            .Where(release => release.IsPublished && release.FileContent != null)
            .OrderByDescending(release => release.VersionSort)
            .Select(release => new AppReleaseInfo(
                release.Version,
                release.Notes,
                release.IsMandatory,
                release.FileSizeBytes,
                release.PublishedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<string?> GetLatestVersionAsync(CancellationToken cancellationToken = default)
    {
        return await masterDbContext.AppReleases
            .AsNoTracking()
            .OrderByDescending(release => release.VersionSort)
            .Select(release => release.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<byte[]?> GetContentAsync(string version, CancellationToken cancellationToken = default)
    {
        long sortKey = AppVersion.SortKey(version);

        return await masterDbContext.AppReleases
            .AsNoTracking()
            .Where(release => release.VersionSort == sortKey && release.FileContent != null)
            .Select(release => release.FileContent)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task PublishAsync(
        string version,
        string fileName,
        byte[] content,
        string? notes,
        bool isMandatory,
        CancellationToken cancellationToken = default)
    {
        version = AppVersion.Normalize(version);
        long sortKey = AppVersion.SortKey(version);

        List<AppRelease> releases = await masterDbContext.AppReleases
            .OrderByDescending(release => release.VersionSort)
            .ToListAsync(cancellationToken);

        // Yalnızca en güncel sürümün dosyası saklanır; eskiler silinir ki
        // veritabanı her yayında ~setup boyutu kadar büyümesin.
        foreach (AppRelease older in releases.Where(release => release.VersionSort < sortKey))
        {
            older.ClearContent();
        }

        AppRelease? target = releases.FirstOrDefault(release => release.VersionSort == sortKey);

        if (target is null)
        {
            target = AppRelease.CreateBaseline(AppVersion.Normalize(version), sortKey);
            masterDbContext.AppReleases.Add(target);
        }

        target.Publish(fileName, content, notes, isMandatory, sortKey);

        await masterDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task EnsureBaselineAsync(string version, CancellationToken cancellationToken = default)
    {
        long sortKey = AppVersion.SortKey(version);
        bool exists = await masterDbContext.AppReleases
            .AnyAsync(release => release.VersionSort == sortKey, cancellationToken);

        if (exists)
        {
            return;
        }

        masterDbContext.AppReleases.Add(AppRelease.CreateBaseline(AppVersion.Normalize(version), sortKey));
        await masterDbContext.SaveChangesAsync(cancellationToken);
    }
}
