using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Category;

namespace Qtemplate.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQuery : IRequest<ApiResponse<List<CategoryDto>>>
{
    public bool OnlyActive { get; set; } = true; // false = admin lấy tất cả
}