using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Review;

namespace Qtemplate.Application.Features.Reviews.Queries.GetMyReviews;

public class GetMyReviewsQuery : IRequest<ApiResponse<List<ReviewDto>>>
{
    public Guid UserId { get; set; }
}