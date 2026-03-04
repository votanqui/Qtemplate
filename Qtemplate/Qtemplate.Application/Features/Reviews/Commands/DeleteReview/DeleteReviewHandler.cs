using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Reviews.Commands.DeleteReview;

public class DeleteReviewHandler : IRequestHandler<DeleteReviewCommand, ApiResponse<object>>
{
    private readonly IReviewRepository _reviewRepo;

    public DeleteReviewHandler(IReviewRepository reviewRepo) => _reviewRepo = reviewRepo;

    public async Task<ApiResponse<object>> Handle(
        DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepo.GetByIdAsync(request.ReviewId);
        if (review is null)
            return ApiResponse<object>.Fail("Không tìm thấy review");

        if (!request.IsAdmin && review.UserId != request.UserId)
            return ApiResponse<object>.Fail("Không có quyền xóa review này");

        var templateId = review.TemplateId;
        await _reviewRepo.DeleteAsync(review);
        await _reviewRepo.UpdateTemplateRatingAsync(templateId);

        return ApiResponse<object>.Ok(null!, "Đã xóa review");
    }
}