using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Templates.Commands.UpdatePreview;

public class UpdatePreviewCommand : IRequest<ApiResponse<object>>
{
    public Guid TemplateId { get; set; }
    public string? PreviewFolder { get; set; }
    public string PreviewType { get; set; } = "None";

    public string AdminId { get; set; } = string.Empty;
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}