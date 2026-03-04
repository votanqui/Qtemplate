using MediatR;
using Microsoft.AspNetCore.Http;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Templates.Commands.AddTemplateImage;

public class AddTemplateImageCommand : IRequest<ApiResponse<object>>
{
    public Guid TemplateId { get; set; }
    public IFormFile File { get; set; } = null!;
    public string? AltText { get; set; }
    public string Type { get; set; } = "Screenshot"; // Screenshot / Banner
    public int SortOrder { get; set; } = 0;
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}