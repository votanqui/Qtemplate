using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Reviews.Commands.DeleteReview;

public class DeleteReviewCommand : IRequest<ApiResponse<object>>
{
    public int ReviewId { get; set; }
    public Guid UserId { get; set; }
    public bool IsAdmin { get; set; } = false;
}