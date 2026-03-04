using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Templates.Commands.SetPreviewUrl;

public class SetPreviewUrlCommand : IRequest<ApiResponse<object>>
{
    public Guid TemplateId { get; set; }
    public string? PreviewUrl { get; set; }
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}