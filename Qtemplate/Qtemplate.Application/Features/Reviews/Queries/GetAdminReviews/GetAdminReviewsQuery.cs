using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Review;

namespace Qtemplate.Application.Features.Reviews.Queries.GetAdminReviews;

public class GetAdminReviewsQuery : IRequest<ApiResponse<PaginatedResult<ReviewDto>>>
{
    public string? Status { get; set; }   // pending / approved / rejected
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}