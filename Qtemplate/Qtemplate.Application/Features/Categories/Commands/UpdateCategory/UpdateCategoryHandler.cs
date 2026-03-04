using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, ApiResponse<object>>
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly IAuditLogService _auditLogService;

    public UpdateCategoryHandler(ICategoryRepository categoryRepo, IAuditLogService auditLogService)
    {
        _categoryRepo = categoryRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<object>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepo.GetByIdAsync(request.Id);
        if (category is null)
            return ApiResponse<object>.Fail("Không tìm thấy danh mục");

        var dto = request.Dto;

        if (category.Slug != dto.Slug && await _categoryRepo.SlugExistsAsync(dto.Slug))
            return ApiResponse<object>.Fail("Slug đã tồn tại");

        if (dto.ParentId == request.Id)
            return ApiResponse<object>.Fail("Danh mục không thể là cha của chính nó");

        var oldValues = new { category.Name, category.Slug, category.ParentId };

        category.ParentId = dto.ParentId == 0 ? null : dto.ParentId;
        category.Name = dto.Name.Trim();
        category.Slug = dto.Slug.Trim().ToLower();
        category.Description = dto.Description?.Trim();
        category.IconUrl = dto.IconUrl;
        category.SortOrder = dto.SortOrder;
        category.IsActive = dto.IsActive;

        await _categoryRepo.UpdateAsync(category);

        await _auditLogService.LogAsync(
            userId: request.AdminId,
            userEmail: request.AdminEmail,
            action: "UpdateCategory",
            entityName: "Category",
            entityId: category.Id.ToString(),
            oldValues: oldValues,
            newValues: new { category.Name, category.Slug, category.ParentId },
            ipAddress: request.IpAddress);

        return ApiResponse<object>.Ok(null!, "Cập nhật danh mục thành công");
    }
}