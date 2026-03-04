using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Review;
using Qtemplate.Application.Features.Reviews.Commands.CreateReview;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Reviews.Commands.UpdateReview;

public class UpdateReviewHandler : IRequestHandler<UpdateReviewCommand, ApiResponse<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepo;
    private readonly IAiModerationService _aiService;

    public UpdateReviewHandler(IReviewRepository reviewRepo, IAiModerationService aiService)
    {
        _reviewRepo = reviewRepo;
        _aiService = aiService;
    }

    public async Task<ApiResponse<ReviewDto>> Handle(
        UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepo.GetByIdAsync(request.ReviewId);
        if (review is null)
            return ApiResponse<ReviewDto>.Fail("Không tìm thấy review");

        if (review.UserId != request.UserId)
            return ApiResponse<ReviewDto>.Fail("Không có quyền sửa review này");

        if (request.Rating is < 1 or > 5)
            return ApiResponse<ReviewDto>.Fail("Rating phải từ 1 đến 5");

        // AI moderation lại khi sửa
        var moderation = await _aiService.ModerateReviewAsync(
            request.Title, request.Comment, request.Rating);

        review.Rating = request.Rating;
        review.Title = request.Title?.Trim();
        review.Comment = request.Comment?.Trim();
        review.AiStatus = moderation.IsApproved ? "Approved" : "Rejected";
        review.AiReason = moderation.Reason;
        review.IsApproved = moderation.IsApproved;
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepo.UpdateAsync(review);
        await _reviewRepo.UpdateTemplateRatingAsync(review.TemplateId);

        return ApiResponse<ReviewDto>.Ok(
            CreateReviewHandler.ToDto(review),
            moderation.IsApproved ? "Đã cập nhật review" : "Review đang chờ admin duyệt");
    }
}