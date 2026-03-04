using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Templates.Commands.ChangeTemplateStatus;

public class ChangeTemplateStatusCommand : IRequest<ApiResponse<object>>
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty; // Draft / Published / Hidden
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}