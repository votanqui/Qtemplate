using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Category;

namespace Qtemplate.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommand : IRequest<ApiResponse<int>>
{
    public CreateCategoryDto Dto { get; set; } = new();
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}