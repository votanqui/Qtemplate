using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Media;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Media.Commands.UploadMedia;

public class UploadMediaHandler : IRequestHandler<UploadMediaCommand, ApiResponse<MediaFileDto>>
{
    private readonly IMediaFileRepository _mediaRepo;
    private readonly ITemplateRepository _templateRepo;
    private readonly IFileUploadService _fileUploadService;

    public UploadMediaHandler(
        IMediaFileRepository mediaRepo,
        ITemplateRepository templateRepo,
        IFileUploadService fileUploadService)
    {
        _mediaRepo = mediaRepo;
        _templateRepo = templateRepo;
        _fileUploadService = fileUploadService;
    }

    public async Task<ApiResponse<MediaFileDto>> Handle(
        UploadMediaCommand request, CancellationToken cancellationToken)
    {
        var file = request.File;
        if (file.Length == 0)
            return ApiResponse<MediaFileDto>.Fail("File rỗng");

        // Xóa MediaFile cũ trong DB nếu template đã có
        if (request.TemplateId.HasValue)
        {
            var existing = await _templateRepo.GetByIdAsync(request.TemplateId.Value);
            if (existing?.MediaFileId is not null)
            {
                var oldMedia = await _mediaRepo.GetByIdAsync(existing.MediaFileId.Value);
                if (oldMedia is not null)
                {
                    // Xóa file vật lý cũ qua service
                    if (oldMedia.StorageType == "Local")
                        _fileUploadService.DeleteDownloadByUrl(oldMedia.Url);

                    await _mediaRepo.DeleteAsync(oldMedia);
                }
            }
        }

        // Lưu file mới qua service — tên file là templateId
        string downloadPath;
        try
        {
            await using var stream = file.OpenReadStream();
            downloadPath = await _fileUploadService.SaveDownloadZipAsync(
                stream, file.FileName, file.Length,
                request.TemplateId ?? Guid.NewGuid());
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse<MediaFileDto>.Fail(ex.Message);
        }

        var mediaFile = new MediaFile
        {
            FileName = $"{(request.TemplateId ?? Guid.NewGuid()):N}.zip",
            OriginalName = file.FileName,
            Url = downloadPath,
            MimeType = file.ContentType,
            FileSize = file.Length,
            StorageType = "Local",
            TemplateId = request.TemplateId,
            UploadedBy = request.AdminId,
            CreatedAt = DateTime.UtcNow
        };

        await _mediaRepo.AddAsync(mediaFile);

        // Gắn vào template
        if (request.TemplateId.HasValue)
        {
            var template = await _templateRepo.GetByIdAsync(request.TemplateId.Value);
            if (template is not null)
            {
                template.MediaFileId = mediaFile.Id;
                template.DownloadPath = mediaFile.Url;
                template.StorageType = "Local";
                template.UpdatedAt = DateTime.UtcNow;
                await _templateRepo.UpdateAsync(template);
            }
        }

        return ApiResponse<MediaFileDto>.Ok(ToDto(mediaFile), "Upload thành công");
    }

    internal static MediaFileDto ToDto(MediaFile m) => new()
    {
        Id = m.Id,
        FileName = m.FileName,
        OriginalName = m.OriginalName,
        Url = m.Url,
        MimeType = m.MimeType,
        FileSize = m.FileSize,
        FileSizeText = FormatSize(m.FileSize),
        StorageType = m.StorageType,
        ExternalId = m.ExternalId,
        TemplateId = m.TemplateId,
        TemplateName = m.Template?.Name,
        CreatedAt = m.CreatedAt
    };

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        < 1024L * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
        _ => $"{bytes / (1024.0 * 1024 * 1024):F1} GB"
    };
}