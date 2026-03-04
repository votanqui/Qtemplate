using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Commands.SetTemplateSale;

public class SetTemplateSaleHandler : IRequestHandler<SetTemplateSaleCommand, ApiResponse<object>>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly IAuditLogService _auditLogService;

    public SetTemplateSaleHandler(ITemplateRepository templateRepo, IAuditLogService auditLogService)
    {
        _templateRepo = templateRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<object>> Handle(SetTemplateSaleCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateRepo.GetByIdAsync(request.TemplateId);
        if (template is null)
            return ApiResponse<object>.Fail("Không tìm thấy template");

        if (template.IsFree)
            return ApiResponse<object>.Fail("Template miễn phí không thể đặt sale");

        // Xóa sale
        if (request.SalePrice is null)
        {
            template.SalePrice = null;
            template.SaleStartAt = null;
            template.SaleEndAt = null;
            template.UpdatedAt = DateTime.UtcNow;
            await _templateRepo.UpdateAsync(template);

            await _auditLogService.LogAsync(
                userId: request.AdminId, userEmail: request.AdminEmail,
                action: "RemoveSale", entityName: "Template",
                entityId: template.Id.ToString(), ipAddress: request.IpAddress);

            return ApiResponse<object>.Ok(null!, "Đã xóa sale");
        }

        // Validate sale
        if (request.SalePrice <= 0)
            return ApiResponse<object>.Fail("Giá sale phải > 0");

        if (request.SalePrice >= template.Price)
            return ApiResponse<object>.Fail($"Giá sale phải nhỏ hơn giá gốc ({template.Price})");

        if (request.SaleStartAt.HasValue && request.SaleEndAt.HasValue
            && request.SaleStartAt >= request.SaleEndAt)
            return ApiResponse<object>.Fail("Ngày bắt đầu phải trước ngày kết thúc");

        var oldValues = new { template.SalePrice, template.SaleStartAt, template.SaleEndAt };

        template.SalePrice = request.SalePrice;
        template.SaleStartAt = request.SaleStartAt;
        template.SaleEndAt = request.SaleEndAt;
        template.UpdatedAt = DateTime.UtcNow;

        await _templateRepo.UpdateAsync(template);

        await _auditLogService.LogAsync(
            userId: request.AdminId, userEmail: request.AdminEmail,
            action: "SetSale", entityName: "Template",
            entityId: template.Id.ToString(),
            oldValues: oldValues,
            newValues: new { template.SalePrice, template.SaleStartAt, template.SaleEndAt },
            ipAddress: request.IpAddress);

        return ApiResponse<object>.Ok(null!, "Cập nhật sale thành công");
    }
}