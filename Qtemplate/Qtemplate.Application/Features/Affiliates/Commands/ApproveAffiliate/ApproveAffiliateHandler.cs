using MediatR;
using Qtemplate.Application.Constants;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Affiliate;
using Qtemplate.Application.Features.Affiliates.Commands.RegisterAffiliate;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Affiliates.Commands.ApproveAffiliate;

public class ApproveAffiliateHandler
    : IRequestHandler<ApproveAffiliateCommand, ApiResponse<AffiliateDto>>
{
    private readonly IAffiliateRepository _affiliateRepo;
    private readonly ISettingRepository _settingRepo;  // 👈 thêm
    private readonly INotificationService _notifService;
    public ApproveAffiliateHandler(
        IAffiliateRepository affiliateRepo,
        ISettingRepository settingRepo,
        INotificationService notifService)
    {
        _affiliateRepo = affiliateRepo;
        _settingRepo = settingRepo;
        _notifService = notifService;
    }

    public async Task<ApiResponse<AffiliateDto>> Handle(
        ApproveAffiliateCommand request, CancellationToken cancellationToken)
    {
        var affiliate = await _affiliateRepo.GetByIdAsync(request.AffiliateId);
        if (affiliate is null)
            return ApiResponse<AffiliateDto>.Fail("Không tìm thấy affiliate");

        // 👈 Đọc commission rate từ Settings, fallback 10%
        var rateStr = await _settingRepo.GetValueAsync(SettingKeys.AffiliateCommissionRate);
        var rate = decimal.TryParse(rateStr, out var parsed) ? parsed : 10m;

        if (rate is < 1 or > 50)
            rate = 10m; // safety fallback

        affiliate.IsActive = request.IsActive;
        affiliate.CommissionRate = rate;  // 👈 dùng rate từ Settings

        await _affiliateRepo.UpdateAsync(affiliate);
        await _notifService.SendToUserAsync(
                affiliate.UserId,
                request.IsActive ? "Affiliate đã được duyệt" : "Affiliate đã bị vô hiệu hóa",
                request.IsActive
                    ? "Tài khoản affiliate của bạn đã được duyệt."
                    : "Affiliate của bạn đã bị tạm ngưng.",
                type: request.IsActive ? "Success" : "Warning",
                redirectUrl: "/dashboard/affiliate"
            );
        return ApiResponse<AffiliateDto>.Ok(
            RegisterAffiliateHandler.ToDto(affiliate),
            request.IsActive ? "Đã duyệt affiliate" : "Đã vô hiệu hoá affiliate");
    }
}