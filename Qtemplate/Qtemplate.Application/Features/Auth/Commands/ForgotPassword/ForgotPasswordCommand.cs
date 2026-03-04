using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommand : IRequest<ApiResponse<object>>
{
    public string Email { get; set; } = string.Empty;
}