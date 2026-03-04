using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Stats;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Stats.Queries.GetMediaStats;

public class GetMediaStatsHandler : IRequestHandler<GetMediaStatsQuery, ApiResponse<MediaStatsDto>>
{
    private readonly IMediaFileRepository _mediaRepo;

    public GetMediaStatsHandler(IMediaFileRepository mediaRepo) => _mediaRepo = mediaRepo;

    public async Task<ApiResponse<MediaStatsDto>> Handle(
        GetMediaStatsQuery request, CancellationToken cancellationToken)
    {
        var files = await _mediaRepo.GetAllAsync();

        var byStorage = files
            .GroupBy(f => f.StorageType)
            .Select(g => new MediaByFolderDto
            {
                StorageType = g.Key,
                Count = g.Count(),
                TotalSize = g.Sum(f => f.FileSize),
                TotalSizeFormatted = FormatSize(g.Sum(f => f.FileSize))
            })
            .OrderByDescending(x => x.TotalSize)
            .ToList();

        var byType = files
            .Where(f => !string.IsNullOrEmpty(f.MimeType))
            .GroupBy(f => f.MimeType!)
            .Select(g => new MediaByTypeDto
            {
                MimeType = g.Key,
                Count = g.Count(),
                TotalSize = g.Sum(f => f.FileSize)
            })
            .OrderByDescending(x => x.TotalSize)
            .ToList();

        var totalSize = files.Sum(f => f.FileSize);

        return ApiResponse<MediaStatsDto>.Ok(new MediaStatsDto
        {
            TotalFiles = files.Count,
            TotalSize = totalSize,
            TotalSizeFormatted = FormatSize(totalSize),
            ByStorage = byStorage,
            ByType = byType
        });
    }

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
        _ => $"{bytes / (1024.0 * 1024 * 1024):F1} GB"
    };
}