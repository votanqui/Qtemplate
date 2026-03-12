using MediatR;
using Qtemplate.Application.DTOs.Affiliate;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Affiliates.Commands.RegisterAffiliate
{
    public class RegisterAffiliateHandler
        : IRequestHandler<RegisterAffiliateCommand, ApiResponse<AffiliateDto>>
    {
        private readonly IAffiliateRepository _affiliateRepo;
        private readonly IUserRepository _userRepo;
        private readonly INotificationService _notifService;

        public RegisterAffiliateHandler(
            IAffiliateRepository affiliateRepo,
            IUserRepository userRepo,
            INotificationService notifService)
        {
            _affiliateRepo = affiliateRepo;
            _userRepo = userRepo;
            _notifService = notifService;
        }

        public async Task<ApiResponse<AffiliateDto>> Handle(
            RegisterAffiliateCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetByIdAsync(request.UserId);
            if (user is null)
                return ApiResponse<AffiliateDto>.Fail("Không tìm thấy người dùng");

            var existing = await _affiliateRepo.GetByUserIdAsync(request.UserId);
            if (existing is not null)
                return ApiResponse<AffiliateDto>.Fail("Bạn đã đăng ký affiliate rồi");

            var code = GenerateCode(user.FullName ?? user.Email);

            var affiliate = new Affiliate
            {
                UserId = request.UserId,
                AffiliateCode = code,
                CommissionRate = 10,
                IsActive = false, // chờ admin approve
                CreatedAt = DateTime.UtcNow
            };

            await _affiliateRepo.AddAsync(affiliate);

            // 🔔 Noti xác nhận đăng ký thành công, chờ duyệt
            await _notifService.SendToUserAsync(
                request.UserId,
                "Đăng ký Affiliate thành công",
                "Yêu cầu affiliate của bạn đã được gửi, vui lòng chờ admin xét duyệt.",
                type: "Info",
                redirectUrl: "/dashboard/affiliate");

            return ApiResponse<AffiliateDto>.Ok(
                ToDto(affiliate, user),
                "Đăng ký affiliate thành công, chờ admin duyệt");
        }

        private static string GenerateCode(string name)
        {
            var clean = new string(name.Where(char.IsLetterOrDigit).ToArray());
            var prefix = clean.Length >= 4
                ? clean[..4].ToUpper()
                : clean.ToUpper().PadRight(4, 'X');
            return $"{prefix}{Random.Shared.Next(1000, 9999)}";
        }

        internal static AffiliateDto ToDto(Affiliate a, Domain.Entities.User? user = null) => new()
        {
            Id = a.Id,
            UserId = a.UserId,
            UserName = user?.FullName ?? a.User?.FullName,
            UserEmail = user?.Email ?? a.User?.Email,
            AffiliateCode = a.AffiliateCode,
            CommissionRate = a.CommissionRate,
            TotalEarned = a.TotalEarned,
            PendingAmount = a.PendingAmount,
            PaidAmount = a.PaidAmount,
            IsActive = a.IsActive,
            CreatedAt = a.CreatedAt,
            Transactions = a.Transactions?
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new AffiliateTransactionDto
                {
                    Id = t.Id,
                    OrderId = t.OrderId,
                    OrderCode = t.Order?.OrderCode,
                    OrderAmount = t.OrderAmount,
                    Commission = t.Commission,
                    Status = t.Status,
                    PaidAt = t.PaidAt,
                    CreatedAt = t.CreatedAt
                }).ToList() ?? new()
        };
    }
}