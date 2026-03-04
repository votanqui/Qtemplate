using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Media.Commands.SetDownloadFile;

public class SetDownloadFileHandler : IRequestHandler<SetDownloadFileCommand, ApiResponse<object>>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly IMediaFileRepository _mediaRepo;

    public SetDownloadFileHandler(ITemplateRepository templateRepo, IMediaFileRepository mediaRepo)
    {
        _templateRepo = templateRepo;
        _mediaRepo = mediaRepo;
    }

    public async Task<ApiResponse<object>> Handle(
        SetDownloadFileCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateRepo.GetByIdAsync(request.TemplateId);
        if (template is null)
            return ApiResponse<object>.Fail("Không tìm thấy template");

        var media = await _mediaRepo.GetByIdAsync(request.MediaFileId);
        if (media is null)
            return ApiResponse<object>.Fail("Không tìm thấy file");

        // Gắn vào template — giống cách UpdatePreviewHandler đang set DownloadPath
        template.MediaFileId = media.Id;
        template.DownloadPath = media.Url;          // giữ DownloadPath để không break code cũ
        template.StorageType = media.StorageType;
        template.UpdatedAt = DateTime.UtcNow;

        // Gắn template vào media
        media.TemplateId = template.Id;

        await _templateRepo.UpdateAsync(template);
        await _mediaRepo.UpdateAsync(media);

        return ApiResponse<object>.Ok(new
        {
            templateId = template.Id,
            mediaFileId = media.Id,
            downloadPath = media.Url,
            storageType = media.StorageType
        }, "Đã gắn file download vào template");
    }
}