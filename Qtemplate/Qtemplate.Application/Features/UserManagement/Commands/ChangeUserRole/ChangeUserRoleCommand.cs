using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Admin.Users.Commands.ChangeUserRole;

public class ChangeUserRoleCommand : IRequest<ApiResponse<object>>
{
    public Guid TargetUserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}