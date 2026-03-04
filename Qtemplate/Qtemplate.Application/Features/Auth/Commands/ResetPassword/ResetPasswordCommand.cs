using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommand : IRequest<ApiResponse<object>>
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}