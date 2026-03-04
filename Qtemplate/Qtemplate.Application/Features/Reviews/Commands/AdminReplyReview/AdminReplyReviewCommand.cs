using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Reviews.Commands.AdminReplyReview;

public class AdminReplyReviewCommand : IRequest<ApiResponse<object>>
{
    public int ReviewId { get; set; }
    public string Reply { get; set; } = string.Empty;
}