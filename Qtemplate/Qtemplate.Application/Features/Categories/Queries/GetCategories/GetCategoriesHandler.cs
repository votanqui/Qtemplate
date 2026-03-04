using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Category;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, ApiResponse<List<CategoryDto>>>
{
    private readonly ICategoryRepository _categoryRepo;

    public GetCategoriesHandler(ICategoryRepository categoryRepo) => _categoryRepo = categoryRepo;

    public async Task<ApiResponse<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepo.GetAllAsync();

        if (request.OnlyActive)
            categories = categories.Where(c => c.IsActive).ToList();

        var dtos = categories.Select(MapToDto).ToList();
        return ApiResponse<List<CategoryDto>>.Ok(dtos);
    }

    private static CategoryDto MapToDto(Category c) => new()
    {
        Id = c.Id,
        ParentId = c.ParentId,
        Name = c.Name,
        Slug = c.Slug,
        Description = c.Description,
        IconUrl = c.IconUrl,
        SortOrder = c.SortOrder,
        IsActive = c.IsActive,
        Children = c.Children.Select(MapToDto).ToList()
    };
}