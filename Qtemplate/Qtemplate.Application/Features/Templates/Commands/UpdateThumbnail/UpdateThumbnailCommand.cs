using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Templates.Commands.UpdateThumbnail;

public class UpdateThumbnailCommand : IRequest<ApiResponse<object>>
{
    public Guid TemplateId { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}