using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Commands.SetPreviewUrl;

public class SetPreviewUrlHandler : IRequestHandler<SetPreviewUrlCommand, ApiResponse<object>>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly IAuditLogService _auditLogService;
    private readonly IFileUploadService _fileUploadService;

    public SetPreviewUrlHandler(
        ITemplateRepository templateRepo,
        IAuditLogService auditLogService,
        IFileUploadService fileUploadService)
    {
        _templateRepo = templateRepo;
        _auditLogService = auditLogService;
        _fileUploadService = fileUploadService;
    }

    public async Task<ApiResponse<object>> Handle(
        SetPreviewUrlCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateRepo.GetByIdAsync(request.TemplateId);
        if (template is null) return ApiResponse<object>.Fail("Không tìm thấy template");

 
        if (template.PreviewType == "Iframe")
            _fileUploadService.DeletePreview(template.Id);

        var oldValues = new { template.PreviewType, template.PreviewUrl, template.PreviewFolder };

        template.PreviewUrl = request.PreviewUrl;
        template.PreviewType = string.IsNullOrEmpty(request.PreviewUrl) ? "None" : "ExternalUrl";
        template.PreviewFolder = null;
  
        template.UpdatedAt = DateTime.UtcNow;

        await _templateRepo.UpdateAsync(template);

        await _auditLogService.LogAsync(
            userId: request.AdminId,
            userEmail: request.AdminEmail,
            action: "SetPreviewUrl",
            entityName: "Template",
            entityId: template.Id.ToString(),
            oldValues: oldValues,
            newValues: new { template.PreviewUrl, template.PreviewType },
            ipAddress: request.IpAddress);

        return ApiResponse<object>.Ok(null!, string.IsNullOrEmpty(request.PreviewUrl)
            ? "Đã xóa preview URL"
            : "Cập nhật preview URL thành công");
    }
}