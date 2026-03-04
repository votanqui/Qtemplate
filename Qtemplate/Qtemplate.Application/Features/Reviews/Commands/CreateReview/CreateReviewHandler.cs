using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Review;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, ApiResponse<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepo;
    private readonly ITemplateRepository _templateRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IAiModerationService _aiService;

    public CreateReviewHandler(
        IReviewRepository reviewRepo,
        ITemplateRepository templateRepo,
        IOrderRepository orderRepo,
        IAiModerationService aiService)
    {
        _reviewRepo = reviewRepo;
        _templateRepo = templateRepo;
        _orderRepo = orderRepo;
        _aiService = aiService;
    }

    public async Task<ApiResponse<ReviewDto>> Handle(
     CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateRepo.GetBySlugAsync(request.Slug);
        if (template is null)
            return ApiResponse<ReviewDto>.Fail("Template không tồn tại");

        var existing = await _reviewRepo.GetByUserAndTemplateAsync(request.UserId, template.Id);
        if (existing is not null)
            return ApiResponse<ReviewDto>.Fail("Bạn đã review template này rồi");

        if (!template.IsFree)
        {
            var order = await _orderRepo.GetPaidOrderByUserAndTemplateAsync(
                request.UserId, template.Id);
            if (order is null)
                return ApiResponse<ReviewDto>.Fail("Bạn cần mua template này trước khi review");
        }

        if (request.Rating is < 1 or > 5)
            return ApiResponse<ReviewDto>.Fail("Rating phải từ 1 đến 5");

        // ── Sync rule-based check — nhanh, không chờ AI ──
        var ruleResult = _aiService.ModerateReviewSync(request.Title, request.Comment);

        var review = new Review
        {
            TemplateId = template.Id,
            UserId = request.UserId,
            Rating = request.Rating,
            Title = request.Title?.Trim(),
            Comment = request.Comment?.Trim(),
            AiStatus = ruleResult.IsApproved ? "Pending" : "Rejected",  // Pending = chờ AI xét
            AiReason = ruleResult.IsApproved ? "Đang chờ AI kiểm duyệt" : ruleResult.Reason,
            IsApproved = false,   // Mặc định false, background job sẽ approve nếu AI pass
            CreatedAt = DateTime.UtcNow
        };

        await _reviewRepo.AddAsync(review);

        return ApiResponse<ReviewDto>.Ok(
            ToDto(review),
            ruleResult.IsApproved
                ? "Review đã được gửi, đang chờ kiểm duyệt tự động"
                : "Review bị từ chối do nội dung không phù hợp");
    }

    internal static ReviewDto ToDto(Review r) => new()
    {
        Id = r.Id,
        TemplateId = r.TemplateId,
        TemplateName = r.Template?.Name,
        TemplateSlug = r.Template?.Slug,
        UserId = r.UserId,
        UserName = r.User?.FullName,
        UserAvatar = r.User?.AvatarUrl,
        Rating = r.Rating,
        Title = r.Title,
        Comment = r.Comment,
        IsApproved = r.IsApproved,
        AdminReply = r.AdminReply,
        AdminRepliedAt = r.AdminRepliedAt,
        AiStatus = r.AiStatus,
        AiReason = r.AiReason,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };
}