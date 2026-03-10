using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Application.Mappers;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Admin.Users.Queries.GetUserList;

public class GetUserListHandler
    : IRequestHandler<GetUserListQuery, ApiResponse<PaginatedResult<AdminUserDto>>>
{
    private readonly IUserRepository _userRepo;
    public GetUserListHandler(IUserRepository userRepo) => _userRepo = userRepo;

    public async Task<ApiResponse<PaginatedResult<AdminUserDto>>> Handle(
        GetUserListQuery request, CancellationToken cancellationToken)
    {
        var (users, total) = await _userRepo.GetPagedAsync(
            request.Search, request.Role, request.IsActive,
            request.Page, request.PageSize);

        return ApiResponse<PaginatedResult<AdminUserDto>>.Ok(new PaginatedResult<AdminUserDto>
        {
            // List không load stats — chỉ trả thông tin cơ bản
            Items = users.Select(u => UserMapper.ToAdminDto(u)).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}