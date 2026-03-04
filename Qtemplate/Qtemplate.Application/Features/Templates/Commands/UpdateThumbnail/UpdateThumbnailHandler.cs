using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Commands.UpdateThumbnail;

public class UpdateThumbnailHandler : IRequestHandler<UpdateThumbnailCommand, ApiResponse<object>>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly IFileUploadService _fileUploadService;
    private readonly IAuditLogService _auditLogService;

    public UpdateThumbnailHandler(ITemplateRepository templateRepo, IFileUploadService fileUploadService, IAuditLogService auditLogService)
    {
        _templateRepo = templateRepo;
        _fileUploadService = fileUploadService;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<object>> Handle(UpdateThumbnailCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateRepo.GetByIdAsync(request.TemplateId);
        if (template is null) return ApiResponse<object>.Fail("Không tìm thấy template");

        _fileUploadService.DeleteThumbnail(template.ThumbnailUrl);

        template.ThumbnailUrl = request.ThumbnailUrl;
        template.UpdatedAt = DateTime.UtcNow;
        await _templateRepo.UpdateAsync(template);

        await _auditLogService.LogAsync(
            userId: request.AdminId, userEmail: request.AdminEmail,
            action: "UpdateThumbnail", entityName: "Template",
            entityId: template.Id.ToString(),
            newValues: new { ThumbnailUrl = request.ThumbnailUrl },
            ipAddress: request.IpAddress);

        return ApiResponse<object>.Ok(null!, "Cập nhật thumbnail thành công");
    }
}