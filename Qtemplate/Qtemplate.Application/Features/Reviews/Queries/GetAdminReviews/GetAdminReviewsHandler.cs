using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Review;
using Qtemplate.Application.Features.Reviews.Commands.CreateReview;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Reviews.Queries.GetAdminReviews;

public class GetAdminReviewsHandler
    : IRequestHandler<GetAdminReviewsQuery, ApiResponse<PaginatedResult<ReviewDto>>>
{
    private readonly IReviewRepository _reviewRepo;

    public GetAdminReviewsHandler(IReviewRepository reviewRepo) => _reviewRepo = reviewRepo;

    public async Task<ApiResponse<PaginatedResult<ReviewDto>>> Handle(
        GetAdminReviewsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _reviewRepo.GetAdminListAsync(
            request.Status, request.Page, request.PageSize);

        return ApiResponse<PaginatedResult<ReviewDto>>.Ok(new PaginatedResult<ReviewDto>
        {
            Items = items.Select(CreateReviewHandler.ToDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}