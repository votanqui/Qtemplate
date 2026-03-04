using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.User;

namespace Qtemplate.Application.Features.UserManagement.Queries.GetProfile;

public class GetProfileQuery : IRequest<ApiResponse<UserProfileDto>>
{
    public Guid UserId { get; set; }
}