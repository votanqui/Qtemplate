using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.User;

namespace Qtemplate.Application.Features.Admin.Users.Queries.GetUserDetail;

public class GetUserDetailQuery : IRequest<ApiResponse<AdminUserDto>>
{
    public Guid UserId { get; set; }
}