using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Reviews.Commands.AdminApproveReview;

public class AdminApproveReviewCommand : IRequest<ApiResponse<object>>
{
    public int ReviewId { get; set; }
    public bool IsApproved { get; set; }
}