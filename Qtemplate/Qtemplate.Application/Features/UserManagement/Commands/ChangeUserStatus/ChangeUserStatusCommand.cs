using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Admin.Users.Commands.ChangeUserStatus;

public class ChangeUserStatusCommand : IRequest<ApiResponse<object>>
{
    public Guid TargetUserId { get; set; }
    public bool IsActive { get; set; }
    public string? Reason { get; set; }
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}