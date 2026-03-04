using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Domain.Messages;

namespace Qtemplate.Application.Features.Reviews.Commands.AdminReplyReview;

public class AdminReplyReviewHandler : IRequestHandler<AdminReplyReviewCommand, ApiResponse<object>>
{
    private readonly IReviewRepository _reviewRepo;
    private readonly IUserRepository _userRepo;
    private readonly IEmailSender _emailSender;

    public AdminReplyReviewHandler(
        IReviewRepository reviewRepo,
        IUserRepository userRepo,
        IEmailSender emailSender)
    {
        _reviewRepo = reviewRepo;
        _userRepo = userRepo;
        _emailSender = emailSender;
    }

    public async Task<ApiResponse<object>> Handle(
        AdminReplyReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepo.GetByIdAsync(request.ReviewId);
        if (review is null)
            return ApiResponse<object>.Fail("Không tìm thấy review");

        review.AdminReply = request.Reply.Trim();
        review.AdminRepliedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;
        await _reviewRepo.UpdateAsync(review);

        // Gửi email thông báo review được phản hồi
        var user = await _userRepo.GetByIdAsync(review.UserId);
        if (user is not null)
        {
            _ = _emailSender.SendAsync(new SendEmailMessage
            {
                To = user.Email,
                Subject = "Review của bạn đã được phản hồi",
                Body = EmailTemplates.ReviewReplied(
                    user.FullName,
                    review.Title ?? "Review của bạn",
                    request.Reply.Trim()),
                Template = "ReviewReplied"
            });
        }

        return ApiResponse<object>.Ok(null!, "Đã trả lời review");
    }
}