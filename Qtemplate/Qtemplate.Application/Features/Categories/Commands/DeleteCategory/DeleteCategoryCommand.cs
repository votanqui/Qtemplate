using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommand : IRequest<ApiResponse<object>>
{
    public int Id { get; set; }
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}