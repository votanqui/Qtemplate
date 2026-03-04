using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.UserManagement.Queries.GetProfile;

public class GetProfileHandler : IRequestHandler<GetProfileQuery, ApiResponse<UserProfileDto>>
{
    private readonly IUserRepository _userRepo;

    public GetProfileHandler(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<ApiResponse<UserProfileDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetActiveByIdAsync(request.UserId);

        if (user is null)
            return ApiResponse<UserProfileDto>.Fail("Tài khoản không tồn tại hoặc đã bị vô hiệu hóa");

        return ApiResponse<UserProfileDto>.Ok(new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role,
            IsEmailVerified = user.IsEmailVerified,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        });
    }
}
