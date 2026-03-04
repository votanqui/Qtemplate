using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.User;

namespace Qtemplate.Application.Features.Auth.Commands.Register;

public class RegisterCommand : IRequest<ApiResponse<AuthResponseDto>>
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}