using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Commands.UpdatePreview;

public class UpdatePreviewHandler : IRequestHandler<UpdatePreviewCommand, ApiResponse<object>>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly IAuditLogService _auditLogService;

    public UpdatePreviewHandler(ITemplateRepository templateRepo, IAuditLogService auditLogService)
    {
        _templateRepo = templateRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<object>> Handle(
        UpdatePreviewCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateRepo.GetByIdAsync(request.TemplateId);
        if (template is null) return ApiResponse<object>.Fail("Không tìm thấy template");

        template.PreviewFolder = request.PreviewFolder;
        template.PreviewType = request.PreviewType;
        template.PreviewUrl = null;
        template.UpdatedAt = DateTime.UtcNow;
  

        await _templateRepo.UpdateAsync(template);

        await _auditLogService.LogAsync(
            userId: request.AdminId,
            userEmail: request.AdminEmail,
            action: "UpdatePreview",
            entityName: "Template",
            entityId: template.Id.ToString(),
            newValues: new { request.PreviewFolder, request.PreviewType },
            ipAddress: request.IpAddress);

        return ApiResponse<object>.Ok(null!, "Cập nhật preview thành công");
    }
}