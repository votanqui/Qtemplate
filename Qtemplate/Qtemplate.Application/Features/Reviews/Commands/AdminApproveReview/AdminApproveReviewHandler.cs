using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Reviews.Commands.AdminApproveReview;

public class AdminApproveReviewHandler : IRequestHandler<AdminApproveReviewCommand, ApiResponse<object>>
{
    private readonly IReviewRepository _reviewRepo;
    private readonly INotificationService _notifService;

    public AdminApproveReviewHandler(
        IReviewRepository reviewRepo,
        INotificationService notifService)
    {
        _reviewRepo = reviewRepo;
        _notifService = notifService;
    }

    public async Task<ApiResponse<object>> Handle(
        AdminApproveReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await _reviewRepo.GetByIdAsync(request.ReviewId);

        if (review is null)
            return ApiResponse<object>.Fail("Không tìm thấy review");

        review.IsApproved = request.IsApproved;
        review.AiStatus = request.IsApproved ? "Approved" : "Rejected";
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepo.UpdateAsync(review);

        // cập nhật rating template
        await _reviewRepo.UpdateTemplateRatingAsync(review.TemplateId);

        // 🔔 gửi notification cho user
        await _notifService.SendToUserAsync(
            review.UserId,
            request.IsApproved ? "Review đã được duyệt" : "Review bị từ chối",
            request.IsApproved
                ? "Review của bạn đã được hiển thị công khai."
                : "Review của bạn không đáp ứng tiêu chuẩn cộng đồng.",
            request.IsApproved ? "Success" : "Warning",
            $"/templates/{review.TemplateId}"
        );

        return ApiResponse<object>.Ok(
            null!,
            request.IsApproved ? "Đã duyệt review" : "Đã từ chối review"
        );
    }
}