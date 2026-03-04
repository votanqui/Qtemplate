using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Review;
using Qtemplate.Application.Features.Reviews.Commands.CreateReview;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Reviews.Queries.GetMyReviews;

public class GetMyReviewsHandler : IRequestHandler<GetMyReviewsQuery, ApiResponse<List<ReviewDto>>>
{
    private readonly IReviewRepository _reviewRepo;

    public GetMyReviewsHandler(IReviewRepository reviewRepo) => _reviewRepo = reviewRepo;

    public async Task<ApiResponse<List<ReviewDto>>> Handle(
        GetMyReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepo.GetByUserIdAsync(request.UserId);
        return ApiResponse<List<ReviewDto>>.Ok(
            reviews.Select(CreateReviewHandler.ToDto).ToList());
    }
}