using MediatR;
using Microsoft.AspNetCore.Http;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.UserManagement.Commands.UpdateAvatar;

public class UpdateAvatarCommand : IRequest<ApiResponse<string>>
{
    public Guid UserId { get; set; }
    public IFormFile File { get; set; } = null!;
    public string? IpAddress { get; set; }
}