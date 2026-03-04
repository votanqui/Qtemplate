using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Admin.Users.Queries.GetUserList;

public class GetUserListHandler
    : IRequestHandler<GetUserListQuery, ApiResponse<PaginatedResult<AdminUserDto>>>
{
    private readonly IUserRepository _userRepo;
    private readonly IOrderRepository _orderRepo;

    public GetUserListHandler(IUserRepository userRepo, IOrderRepository orderRepo)
    {
        _userRepo = userRepo;
        _orderRepo = orderRepo;
    }

    public async Task<ApiResponse<PaginatedResult<AdminUserDto>>> Handle(
        GetUserListQuery request, CancellationToken cancellationToken)
    {
        var (users, total) = await _userRepo.GetPagedAsync(
            request.Search, request.Role, request.IsActive,
            request.Page, request.PageSize);

        var dtos = users.Select(u => new AdminUserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            AvatarUrl = u.AvatarUrl,
            Role = u.Role,
            IsActive = u.IsActive,
            IsEmailVerified = u.IsEmailVerified,
            LastLoginAt = u.LastLoginAt,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        }).ToList();

        return ApiResponse<PaginatedResult<AdminUserDto>>.Ok(new PaginatedResult<AdminUserDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}