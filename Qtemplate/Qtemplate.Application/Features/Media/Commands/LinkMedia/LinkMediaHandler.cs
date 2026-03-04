using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Media;
using Qtemplate.Application.Features.Media.Commands.UploadMedia;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Media.Commands.LinkMedia;

public class LinkMediaHandler : IRequestHandler<LinkMediaCommand, ApiResponse<MediaFileDto>>
{
    private readonly IMediaFileRepository _mediaRepo;
    private readonly ITemplateRepository _templateRepo;   // ← thêm

    public LinkMediaHandler(
        IMediaFileRepository mediaRepo,
        ITemplateRepository templateRepo)
    {
        _mediaRepo = mediaRepo;
        _templateRepo = templateRepo;
    }

    public async Task<ApiResponse<MediaFileDto>> Handle(
        LinkMediaCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Url))
            return ApiResponse<MediaFileDto>.Fail("URL không được để trống");

        var allowed = new[] { "GoogleDrive", "S3", "R2" };
        if (!allowed.Contains(request.StorageType))
            return ApiResponse<MediaFileDto>.Fail("StorageType không hợp lệ");

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
            return ApiResponse<MediaFileDto>.Fail("URL không hợp lệ");

        var fileName = string.IsNullOrEmpty(request.OriginalName)
            ? Path.GetFileName(request.Url.Split('?')[0])
            : request.OriginalName;

        if (string.IsNullOrEmpty(fileName))
            fileName = $"file_{DateTime.UtcNow:yyyyMMddHHmmss}.zip";

        var mediaFile = new MediaFile
        {
            FileName = fileName,
            OriginalName = request.OriginalName,
            Url = request.Url,
            StorageType = request.StorageType,
            ExternalId = request.ExternalId,
            TemplateId = request.TemplateId,
            UploadedBy = request.AdminId,
            CreatedAt = DateTime.UtcNow
        };

        await _mediaRepo.AddAsync(mediaFile);

        // Tự động gắn vào template nếu có templateId
        if (request.TemplateId.HasValue)
        {
            var template = await _templateRepo.GetByIdAsync(request.TemplateId.Value);
            if (template is not null)
            {
                template.MediaFileId = mediaFile.Id;
                template.DownloadPath = mediaFile.Url;
                template.StorageType = mediaFile.StorageType;
                template.UpdatedAt = DateTime.UtcNow;
                await _templateRepo.UpdateAsync(template);
            }
        }

        return ApiResponse<MediaFileDto>.Ok(
            UploadMediaHandler.ToDto(mediaFile), "Đã lưu link và gắn vào template thành công");
    }
}