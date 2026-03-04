using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Templates.Commands.DeleteTemplateImage;

public class DeleteTemplateImageCommand : IRequest<ApiResponse<object>>
{
    public int ImageId { get; set; }
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}