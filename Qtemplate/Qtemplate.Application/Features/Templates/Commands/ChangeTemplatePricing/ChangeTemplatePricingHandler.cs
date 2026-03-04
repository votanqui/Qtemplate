using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Commands.ChangeTemplatePricing;

public class ChangeTemplatePricingHandler : IRequestHandler<ChangeTemplatePricingCommand, ApiResponse<object>>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly IAuditLogService _auditLogService;

    public ChangeTemplatePricingHandler(ITemplateRepository templateRepo, IAuditLogService auditLogService)
    {
        _templateRepo = templateRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<object>> Handle(ChangeTemplatePricingCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateRepo.GetByIdAsync(request.TemplateId);
        if (template is null)
            return ApiResponse<object>.Fail("Không tìm thấy template");

        // Không đổi gì thì thông báo luôn
        if (template.IsFree == request.IsFree && template.Price == request.Price)
            return ApiResponse<object>.Fail("Không có thay đổi nào");

        if (request.IsFree)
        {
            // Chuyển sang free → xóa sale, set giá = 0
            var oldFreeValues = new { template.IsFree, template.Price, template.SalePrice };

            template.IsFree = true;
            template.Price = 0;
            template.SalePrice = null;
            template.SaleStartAt = null;
            template.SaleEndAt = null;
            template.UpdatedAt = DateTime.UtcNow;

            await _templateRepo.UpdateAsync(template);

            await _auditLogService.LogAsync(
                userId: request.AdminId, userEmail: request.AdminEmail,
                action: "ChangeTemplatePricing", entityName: "Template",
                entityId: template.Id.ToString(),
                oldValues: oldFreeValues,
                newValues: new { IsFree = true, Price = 0 },
                ipAddress: request.IpAddress);

            return ApiResponse<object>.Ok(null!, "Đã chuyển template sang miễn phí");
        }
        else
        {
            // Chuyển sang paid
            if (request.Price <= 0)
                return ApiResponse<object>.Fail("Giá phải > 0 khi chuyển sang trả phí");

            var oldPaidValues = new { template.IsFree, template.Price };

            template.IsFree = false;
            template.Price = request.Price;
            template.UpdatedAt = DateTime.UtcNow;

            await _templateRepo.UpdateAsync(template);

            await _auditLogService.LogAsync(
                userId: request.AdminId, userEmail: request.AdminEmail,
                action: "ChangeTemplatePricing", entityName: "Template",
                entityId: template.Id.ToString(),
                oldValues: oldPaidValues,
                newValues: new { IsFree = false, request.Price },
                ipAddress: request.IpAddress);

            return ApiResponse<object>.Ok(null!, $"Đã chuyển template sang trả phí với giá {request.Price}");
        }
    }
}