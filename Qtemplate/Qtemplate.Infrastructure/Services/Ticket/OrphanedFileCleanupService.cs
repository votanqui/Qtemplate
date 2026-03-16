// File: Qtemplate.Infrastructure/Services/Cleanup/OrphanedFileCleanupService.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qtemplate.Application.Constants;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Services.Cleanup;

public class OrphanedFileCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<OrphanedFileCleanupService> _logger;
    private readonly string _webRootPath;
    private readonly string _privateStoragePath;

    // Không xóa file mới hơn 24h — tránh race condition khi đang upload
    private static readonly TimeSpan SafeAge = TimeSpan.FromHours(24);

    public OrphanedFileCleanupService(
        IServiceProvider services,
        ILogger<OrphanedFileCleanupService> logger,
        IHostEnvironment env)
    {
        _services = services;
        _logger = logger;
        _webRootPath = Path.Combine(env.ContentRootPath, "wwwroot");
        _privateStoragePath = Path.Combine(env.ContentRootPath, "private-storage");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrphanedFileCleanupService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = ComputeDelayUntilSunday(hour: 5, minute: 0);
                _logger.LogInformation(
                    "OrphanedFileCleanupService: next run in {Hours:F1} hours.", delay.TotalHours);

                await Task.Delay(delay, stoppingToken);
                await ProcessAsync();
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OrphanedFileCleanupService error.");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        _logger.LogInformation("OrphanedFileCleanupService stopped.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    private async Task ProcessAsync()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var settingRepo = scope.ServiceProvider.GetRequiredService<ISettingRepository>();
        var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();

        var dryRunSetting = await settingRepo.GetValueAsync(SettingKeys.OrphanedFileDryRun) ?? "true";
        bool dryRun = !string.Equals(dryRunSetting, "false", StringComparison.OrdinalIgnoreCase);

        var safeBeforeDate = DateTime.UtcNow.Subtract(SafeAge);
        var summary = new List<string>();
        int totalOrphaned = 0;
        int totalDeleted = 0;

        // ── 1. Thumbnails ─────────────────────────────────────────────────────
        // ToHashSetAsync không có trong EF Core — dùng ToListAsync rồi new HashSet<>()
        var dbThumbnails = new HashSet<string>(
            await db.Templates
                .Where(t => t.ThumbnailUrl != null)
                .Select(t => Path.GetFileName(t.ThumbnailUrl!))
                .ToListAsync());

        (int oThumbs, int dThumbs) = await CleanFolderAsync(
            Path.Combine(_webRootPath, "thumbnails"),
            dbThumbnails, safeBeforeDate, dryRun);
        totalOrphaned += oThumbs; totalDeleted += dThumbs;
        summary.Add($"thumbnails: {oThumbs} orphaned, {dThumbs} deleted");

        // ── 2. Template images ────────────────────────────────────────────────
        var dbTemplateImages = new HashSet<string>(
            await db.TemplateImages
                .Select(i => Path.GetFileName(i.ImageUrl))
                .ToListAsync());

        (int oImgs, int dImgs) = await CleanFolderAsync(
            Path.Combine(_webRootPath, "template-images"),
            dbTemplateImages, safeBeforeDate, dryRun);
        totalOrphaned += oImgs; totalDeleted += dImgs;
        summary.Add($"template-images: {oImgs} orphaned, {dImgs} deleted");

        // ── 3. Banner images ──────────────────────────────────────────────────
        var dbBanners = new HashSet<string>(
            await db.Banners
                .Where(b => b.ImageUrl != null)
                .Select(b => Path.GetFileName(b.ImageUrl!))
                .ToListAsync());

        (int oBanners, int dBanners) = await CleanFolderAsync(
            Path.Combine(_webRootPath, "banners"),
            dbBanners, safeBeforeDate, dryRun);
        totalOrphaned += oBanners; totalDeleted += dBanners;
        summary.Add($"banners: {oBanners} orphaned, {dBanners} deleted");

        // ── 4. Preview folders (private-storage/previews/{templateId}/) ───────
        var dbPreviewIds = new HashSet<string>(
            await db.Templates
                .Where(t => t.PreviewFolder != null)
                .Select(t => t.Id.ToString())
                .ToListAsync());

        (int oPreviews, int dPreviews) = await CleanSubfoldersAsync(
            Path.Combine(_privateStoragePath, "previews"),
            dbPreviewIds, safeBeforeDate, dryRun);
        totalOrphaned += oPreviews; totalDeleted += dPreviews;
        summary.Add($"preview-folders: {oPreviews} orphaned, {dPreviews} deleted");

        // ── 5. Download zips (private-storage/downloads/{templateId:N}.zip) ───
        var dbDownloadFiles = new HashSet<string>(
            await db.Templates
                .Where(t => t.DownloadPath != null)
                .Select(t => t.Id.ToString("N") + ".zip")
                .ToListAsync());

        (int oDownloads, int dDownloads) = await CleanFolderAsync(
            Path.Combine(_privateStoragePath, "downloads"),
            dbDownloadFiles, safeBeforeDate, dryRun);
        totalOrphaned += oDownloads; totalDeleted += dDownloads;
        summary.Add($"downloads: {oDownloads} orphaned, {dDownloads} deleted");

        // ── 6. Version folders (private-storage/versions/{templateId}/) ───────
        var dbVersionIds = new HashSet<string>(
            await db.TemplateVersions
                .Select(v => v.TemplateId.ToString())
                .Distinct()
                .ToListAsync());

        (int oVersions, int dVersions) = await CleanSubfoldersAsync(
            Path.Combine(_privateStoragePath, "versions"),
            dbVersionIds, safeBeforeDate, dryRun);
        totalOrphaned += oVersions; totalDeleted += dVersions;
        summary.Add($"version-folders: {oVersions} orphaned, {dVersions} deleted");

        // ── Log kết quả ───────────────────────────────────────────────────────
        var mode = dryRun ? "DRY-RUN" : "ACTUAL";
        _logger.LogInformation(
            "OrphanedFileCleanupService [{Mode}]: {Total} orphaned, {Deleted} deleted. {Detail}",
            mode, totalOrphaned, totalDeleted, string.Join(" | ", summary));

        if (totalOrphaned > 0)
        {
            await auditLogService.LogAsync(
                userId: "SYSTEM",
                userEmail: "orphan-cleanup@system",
                action: "OrphanedFileCleanup",
                entityName: "FileSystem",
                entityId: "BATCH",
                newValues: new { mode, totalOrphaned, totalDeleted, summary });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Explicit return type Task<(int, int)> thay vì để compiler infer từ ValueTuple
    private async Task<(int orphaned, int deleted)> CleanFolderAsync(
        string folderPath,
        HashSet<string> knownFileNames,
        DateTime safeBeforeDate,
        bool dryRun)
    {
        if (!Directory.Exists(folderPath)) return (0, 0);

        int orphaned = 0, deleted = 0;

        foreach (var filePath in Directory.GetFiles(folderPath))
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.LastWriteTimeUtc >= safeBeforeDate) continue;
            if (knownFileNames.Contains(fileInfo.Name)) continue;

            orphaned++;
            _logger.LogWarning(
                "OrphanedFileCleanupService: orphaned file {File} (modified {Modified:yyyy-MM-dd})",
                filePath, fileInfo.LastWriteTimeUtc);

            if (!dryRun)
            {
                try { fileInfo.Delete(); deleted++; }
                catch (Exception ex)
                { _logger.LogError(ex, "OrphanedFileCleanupService: failed to delete {File}", filePath); }
            }
        }

        await Task.CompletedTask;
        return (orphaned, deleted);
    }

    // ─────────────────────────────────────────────────────────────────────────
    private async Task<(int orphaned, int deleted)> CleanSubfoldersAsync(
        string parentPath,
        HashSet<string> knownIds,
        DateTime safeBeforeDate,
        bool dryRun)
    {
        if (!Directory.Exists(parentPath)) return (0, 0);

        int orphaned = 0, deleted = 0;

        foreach (var subDir in Directory.GetDirectories(parentPath))
        {
            var dirInfo = new DirectoryInfo(subDir);
            if (dirInfo.LastWriteTimeUtc >= safeBeforeDate) continue;
            if (knownIds.Contains(dirInfo.Name)) continue;

            orphaned++;
            _logger.LogWarning(
                "OrphanedFileCleanupService: orphaned folder {Dir} (modified {Modified:yyyy-MM-dd})",
                subDir, dirInfo.LastWriteTimeUtc);

            if (!dryRun)
            {
                try { dirInfo.Delete(recursive: true); deleted++; }
                catch (Exception ex)
                { _logger.LogError(ex, "OrphanedFileCleanupService: failed to delete folder {Dir}", subDir); }
            }
        }

        await Task.CompletedTask;
        return (orphaned, deleted);
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static TimeSpan ComputeDelayUntilSunday(int hour, int minute)
    {
        var now = DateTime.UtcNow;
        var target = now.Date.AddHours(hour).AddMinutes(minute);

        int daysUntilSunday = ((int)DayOfWeek.Sunday - (int)now.DayOfWeek + 7) % 7;
        if (daysUntilSunday == 0 && target <= now) daysUntilSunday = 7;
        target = target.AddDays(daysUntilSunday);

        return target - now;
    }
}