using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Review;

namespace Qtemplate.Application.Features.Reviews.Commands.UpdateReview;

public class UpdateReviewCommand : IRequest<ApiResponse<ReviewDto>>
{
    public int ReviewId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
}