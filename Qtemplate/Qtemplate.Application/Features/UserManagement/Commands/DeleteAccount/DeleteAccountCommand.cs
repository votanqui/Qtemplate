using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.UserManagement.Commands.DeleteAccount;

public class DeleteAccountCommand : IRequest<ApiResponse<bool>>
{
    public Guid UserId { get; set; }
    public string Password { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
}