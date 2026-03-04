using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommand : IRequest<ApiResponse<object>>
{
    public string Token { get; set; } = string.Empty;
}