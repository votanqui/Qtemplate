using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.User;

namespace Qtemplate.Application.Features.Admin.Users.Queries.GetUserList;

public class GetUserListQuery : IRequest<ApiResponse<PaginatedResult<AdminUserDto>>>
{
    public string? Search { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}