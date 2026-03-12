using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Commands.BulkSetSale;

public class BulkSetSaleHandler : IRequestHandler<BulkSetSaleCommand, ApiResponse<object>>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notifService;

    public BulkSetSaleHandler(
        ITemplateRepository templateRepo,
        IAuditLogService auditLogService,
        INotificationService notifService)
    {
        _templateRepo = templateRepo;
        _auditLogService = auditLogService;
        _notifService = notifService;
    }

    public async Task<ApiResponse<object>> Handle(
        BulkSetSaleCommand request, CancellationToken cancellationToken)
    {
        if (request.TemplateIds is null || request.TemplateIds.Count == 0)
            return ApiResponse<object>.Fail("Chưa chọn template nào.");

        if (request.SalePrice.HasValue)
        {
            if (request.SalePrice <= 0)
                return ApiResponse<object>.Fail("Giá sale phải > 0.");

            if (request.SaleStartAt.HasValue && request.SaleEndAt.HasValue
                && request.SaleStartAt >= request.SaleEndAt)
                return ApiResponse<object>.Fail("Ngày bắt đầu phải trước ngày kết thúc.");
        }

        var updatedCount = await _templateRepo.BulkSetSaleAsync(
            request.TemplateIds,
            request.SalePrice,
            request.SaleStartAt,
            request.SaleEndAt);

        if (updatedCount == 0)
            return ApiResponse<object>.Fail(
                "Không có template hợp lệ (template miễn phí hoặc giá sale ≥ giá gốc sẽ bị bỏ qua).");

        await _auditLogService.LogAsync(
            userId: request.AdminId,
            userEmail: request.AdminEmail,
            action: request.SalePrice.HasValue ? "BulkSetSale" : "BulkRemoveSale",
            entityName: "Template",
            entityId: string.Join(",", request.TemplateIds),
            newValues: new { updatedCount, salePrice = request.SalePrice },
            ipAddress: request.IpAddress);

        if (request.SalePrice.HasValue)
            await _notifService.BroadcastAsync(
                $"🔥 {updatedCount} template đang giảm giá!",
                $"Chỉ còn từ {request.SalePrice:N0}đ. Nhanh tay!",
                type: "Info",
                redirectUrl: "/templates");

        var action = request.SalePrice.HasValue ? "Đặt sale" : "Xóa sale";
        return ApiResponse<object>.Ok(
            new { updatedCount },
            $"{action} thành công {updatedCount}/{request.TemplateIds.Count} template.");
    }
}