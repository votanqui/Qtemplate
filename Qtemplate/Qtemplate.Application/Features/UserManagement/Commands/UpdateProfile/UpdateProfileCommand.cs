using MediatR;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Application.DTOs;

public class UpdateProfileCommand : IRequest<ApiResponse<UserProfileDto>>
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? IpAddress { get; set; }
}