using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Commands.DeleteTemplate;

public class DeleteTemplateHandler : IRequestHandler<DeleteTemplateCommand, ApiResponse<object>>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly ITemplateVersionRepository _versionRepo;
    private readonly ITemplateImageRepository _imageRepo;
    private readonly IFileUploadService _fileUploadService;
    private readonly IAuditLogService _auditLogService;

    public DeleteTemplateHandler(
        ITemplateRepository templateRepo,
        ITemplateVersionRepository versionRepo,
        ITemplateImageRepository imageRepo,
        IFileUploadService fileUploadService,
        IAuditLogService auditLogService)
    {
        _templateRepo = templateRepo;
        _versionRepo = versionRepo;
        _imageRepo = imageRepo;
        _fileUploadService = fileUploadService;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<object>> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateRepo.GetByIdFullAsync(request.Id);
        if (template is null)
            return ApiResponse<object>.Fail("Không tìm thấy template");

        if (template.SalesCount > 0)
            return ApiResponse<object>.Fail("Không thể xóa template đã có người mua, hãy chuyển sang Hidden thay thế");

        // Xóa files vật lý
        _fileUploadService.DeletePreview(template.Id);
        _fileUploadService.DeleteDownloadFile(template.Id);
        _fileUploadService.DeleteThumbnail(template.ThumbnailUrl);

        // Xóa version files
        var versions = await _versionRepo.GetByTemplateIdAsync(template.Id);
        foreach (var v in versions)
        {
            var versionPath = _fileUploadService.GetDownloadPhysicalPath(template.Id, v.DownloadPath);
            if (File.Exists(versionPath))
                File.Delete(versionPath);
        }

        // Xóa ảnh screenshots
        foreach (var img in template.Images)
            _fileUploadService.DeleteTemplateImage(img.ImageUrl);

        // Xóa children trong DB trước
        await _templateRepo.DeleteChildrenAsync(template.Id);

        // Xóa template
        await _templateRepo.DeleteAsync(template);

        await _auditLogService.LogAsync(
            userId: request.AdminId,
            userEmail: request.AdminEmail,
            action: "DeleteTemplate",
            entityName: "Template",
            entityId: template.Id.ToString(),
            oldValues: new { template.Name, template.Slug },
            ipAddress: request.IpAddress);

        return ApiResponse<object>.Ok(null!, "Xóa template thành công");
    }
}