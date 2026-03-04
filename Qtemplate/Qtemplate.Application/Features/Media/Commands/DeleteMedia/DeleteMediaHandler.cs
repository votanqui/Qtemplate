using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Media.Commands.DeleteMedia;

public class DeleteMediaHandler : IRequestHandler<DeleteMediaCommand, ApiResponse<object>>
{
    private readonly IMediaFileRepository _mediaRepo;
    private readonly ITemplateRepository _templateRepo;
    private readonly IFileUploadService _fileUploadService;

    public DeleteMediaHandler(
        IMediaFileRepository mediaRepo,
        ITemplateRepository templateRepo,
        IFileUploadService fileUploadService)
    {
        _mediaRepo = mediaRepo;
        _templateRepo = templateRepo;
        _fileUploadService = fileUploadService;
    }

    public async Task<ApiResponse<object>> Handle(
        DeleteMediaCommand request, CancellationToken cancellationToken)
    {
        var media = await _mediaRepo.GetByIdAsync(request.MediaFileId);
        if (media is null)
            return ApiResponse<object>.Fail("Không tìm thấy file");

        // Gỡ liên kết template
        if (media.TemplateId.HasValue)
        {
            var template = await _templateRepo.GetByIdAsync(media.TemplateId.Value);
            if (template is not null && template.MediaFileId == media.Id)
            {
                template.MediaFileId = null;
                template.DownloadPath = null;
                template.StorageType = "Local";
                template.UpdatedAt = DateTime.UtcNow;
                await _templateRepo.UpdateAsync(template);
            }
        }

        // Xóa file vật lý qua service
        if (media.StorageType == "Local")
            _fileUploadService.DeleteDownloadByUrl(media.Url);

        await _mediaRepo.DeleteAsync(media);
        return ApiResponse<object>.Ok(null!, "Đã xóa file");
    }
}