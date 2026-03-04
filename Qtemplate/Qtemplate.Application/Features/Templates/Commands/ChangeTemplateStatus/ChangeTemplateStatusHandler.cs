using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Commands.ChangeTemplateStatus;

public class ChangeTemplateStatusHandler : IRequestHandler<ChangeTemplateStatusCommand, ApiResponse<object>>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly IAuditLogService _auditLogService;

    private static readonly string[] AllowedStatuses = { "Draft", "Published", "Hidden" };

    public ChangeTemplateStatusHandler(ITemplateRepository templateRepo, IAuditLogService auditLogService)
    {
        _templateRepo = templateRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<object>> Handle(ChangeTemplateStatusCommand request, CancellationToken cancellationToken)
    {
        if (!AllowedStatuses.Contains(request.Status))
            return ApiResponse<object>.Fail($"Trạng thái không hợp lệ. Chỉ chấp nhận: {string.Join(", ", AllowedStatuses)}");

        var template = await _templateRepo.GetByIdAsync(request.Id);
        if (template is null)
            return ApiResponse<object>.Fail("Không tìm thấy template");

        if (template.Status == request.Status)
            return ApiResponse<object>.Fail($"Template đang ở trạng thái {request.Status} rồi");

        var oldStatus = template.Status;

        template.Status = request.Status;
        template.UpdatedAt = DateTime.UtcNow;

        // Nếu publish thì set PublishedAt, nếu unpublish thì giữ nguyên PublishedAt
        if (request.Status == "Published" && template.PublishedAt is null)
            template.PublishedAt = DateTime.UtcNow;

        await _templateRepo.UpdateAsync(template);

        await _auditLogService.LogAsync(
            userId: request.AdminId,
            userEmail: request.AdminEmail,
            action: "ChangeTemplateStatus",
            entityName: "Template",
            entityId: template.Id.ToString(),
            oldValues: new { Status = oldStatus },
            newValues: new { Status = request.Status },
            ipAddress: request.IpAddress);

        return ApiResponse<object>.Ok(null!, $"Đã chuyển trạng thái từ {oldStatus} → {request.Status}");
    }
}