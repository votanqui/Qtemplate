using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Auth.Commands.ResendVerifyEmail;

public class ResendVerifyEmailCommand : IRequest<ApiResponse<object>>
{
    public string Email { get; set; } = string.Empty;
}
