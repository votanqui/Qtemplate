using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Review;
using Qtemplate.Application.Features.Reviews.Commands.CreateReview;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Reviews.Queries.GetTemplateReviews;

public class GetTemplateReviewsHandler
    : IRequestHandler<GetTemplateReviewsQuery, ApiResponse<PaginatedResult<ReviewDto>>>
{
    private readonly IReviewRepository _reviewRepo;

    public GetTemplateReviewsHandler(IReviewRepository reviewRepo) => _reviewRepo = reviewRepo;

    public async Task<ApiResponse<PaginatedResult<ReviewDto>>> Handle(
        GetTemplateReviewsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _reviewRepo.GetByTemplateSlugAsync(
            request.Slug, request.Page, request.PageSize);

        return ApiResponse<PaginatedResult<ReviewDto>>.Ok(new PaginatedResult<ReviewDto>
        {
            Items = items.Select(CreateReviewHandler.ToDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}