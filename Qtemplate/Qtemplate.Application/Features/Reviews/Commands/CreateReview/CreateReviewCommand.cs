using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Review;

namespace Qtemplate.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewCommand : IRequest<ApiResponse<ReviewDto>>
{
    public string Slug { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
}