using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Category;

namespace Qtemplate.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommand : IRequest<ApiResponse<object>>
{
    public int Id { get; set; }
    public CreateCategoryDto Dto { get; set; } = new();
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}