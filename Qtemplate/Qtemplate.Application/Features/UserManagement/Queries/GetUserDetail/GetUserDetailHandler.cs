using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.User;
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

        // Lấy stats đơn hàng
        var (orders, _) = await _orderRepo.GetPagedByUserIdAsync(request.UserId, 1, 1000);
        var paidOrders = orders.Where(o => o.Status is "Paid" or "Completed").ToList();

        return ApiResponse<AdminUserDto>.Ok(new AdminUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role,
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            TotalOrders = paidOrders.Count,
            TotalSpent = paidOrders.Sum(o => o.FinalAmount)
        });
    }
}