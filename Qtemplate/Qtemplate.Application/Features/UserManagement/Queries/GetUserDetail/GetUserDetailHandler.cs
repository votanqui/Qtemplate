using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Application.Mappers;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Admin.Users.Queries.GetUserDetail;

public class GetUserDetailHandler
    : IRequestHandler<GetUserDetailQuery, ApiResponse<AdminUserDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IOrderRepository _orderRepo;

    public GetUserDetailHandler(IUserRepository userRepo, IOrderRepository orderRepo)
    {
        _userRepo = userRepo;
        _orderRepo = orderRepo;
    }

    public async Task<ApiResponse<AdminUserDto>> Handle(
        GetUserDetailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId);
        if (user is null)
            return ApiResponse<AdminUserDto>.Fail("Không tìm thấy người dùng");

        // Dùng method tối ưu thay vì load 1000 orders
        var (totalOrders, totalSpent) = await _orderRepo.GetUserStatsAsync(request.UserId);

        return ApiResponse<AdminUserDto>.Ok(
            UserMapper.ToAdminDto(user, totalOrders, totalSpent));
    }
}