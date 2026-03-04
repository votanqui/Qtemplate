using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Auth.Commands.Logout;

public class LogoutCommand : IRequest<ApiResponse<object>>
{
    public string RefreshToken { get; set; } = string.Empty;
}