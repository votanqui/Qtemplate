using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, ApiResponse<int>>
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly IAuditLogService _auditLogService;

    public CreateCategoryHandler(ICategoryRepository categoryRepo, IAuditLogService auditLogService)
    {
        _categoryRepo = categoryRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<int>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        if (await _categoryRepo.SlugExistsAsync(dto.Slug))
            return ApiResponse<int>.Fail("Slug đã tồn tại");

        var category = new Category
        {
            ParentId = dto.ParentId == 0 ? null : dto.ParentId,
            Name = dto.Name.Trim(),
            Slug = dto.Slug.Trim().ToLower(),
            Description = dto.Description?.Trim(),
            IconUrl = dto.IconUrl,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _categoryRepo.AddAsync(category);

        await _auditLogService.LogAsync(
            userId: request.AdminId,
            userEmail: request.AdminEmail,
            action: "CreateCategory",
            entityName: "Category",
            entityId: category.Id.ToString(),
            newValues: new { category.Name, category.Slug, category.ParentId },
            ipAddress: request.IpAddress);

        return ApiResponse<int>.Ok(category.Id, "Tạo danh mục thành công");
    }
}