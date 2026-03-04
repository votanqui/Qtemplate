using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, ApiResponse<object>>
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly IAuditLogService _auditLogService;

    public DeleteCategoryHandler(ICategoryRepository categoryRepo, IAuditLogService auditLogService)
    {
        _categoryRepo = categoryRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<object>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepo.GetByIdAsync(request.Id);
        if (category is null)
            return ApiResponse<object>.Fail("Không tìm thấy danh mục");

        if (category.Children.Any())
            return ApiResponse<object>.Fail("Không thể xóa danh mục đang có danh mục con");

        await _categoryRepo.DeleteAsync(category);

        await _auditLogService.LogAsync(
            userId: request.AdminId,
            userEmail: request.AdminEmail,
            action: "DeleteCategory",
            entityName: "Category",
            entityId: category.Id.ToString(),
            oldValues: new { category.Name, category.Slug },
            ipAddress: request.IpAddress);

        return ApiResponse<object>.Ok(null!, "Xóa danh mục thành công");
    }
}