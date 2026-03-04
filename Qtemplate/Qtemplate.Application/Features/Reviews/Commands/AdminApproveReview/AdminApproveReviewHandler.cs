using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Reviews.Commands.AdminApproveReview;

public class AdminApproveReviewHandler : IRequestHandler<AdminApproveReviewCommand, ApiResponse<object>>
{
    private readonly IReviewRepository _reviewRepo;

    public AdminApproveReviewHandler(IReviewRepository reviewRepo) => _reviewRepo = reviewRepo;

    public async Task<ApiResponse<object>> Handle(
        AdminApproveReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepo.GetByIdAsync(request.ReviewId);
        if (review is null)
            return ApiResponse<object>.Fail("Không tìm thấy review");

        review.IsApproved = request.IsApproved;
        review.AiStatus = request.IsApproved ? "Approved" : "Rejected";
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepo.UpdateAsync(review);
        await _reviewRepo.UpdateTemplateRatingAsync(review.TemplateId);

        return ApiResponse<object>.Ok(null!,
            request.IsApproved ? "Đã duyệt review" : "Đã từ chối review");
    }
}