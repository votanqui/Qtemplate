using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Review;

namespace Qtemplate.Application.Features.Reviews.Queries.GetTemplateReviews;

public class GetTemplateReviewsQuery : IRequest<ApiResponse<PaginatedResult<ReviewDto>>>
{
    public string Slug { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}