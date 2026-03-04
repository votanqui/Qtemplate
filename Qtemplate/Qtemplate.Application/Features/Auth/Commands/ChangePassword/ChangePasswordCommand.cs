using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommand : IRequest<ApiResponse<object>>
{
    public Guid UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
}