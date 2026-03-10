using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Template;
using Qtemplate.Application.Mappers;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Queries.GetTemplateDetail;

public class GetTemplateDetailHandler : IRequestHandler<GetTemplateDetailQuery, ApiResponse<TemplateDetailDto>>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly IWishlistRepository _wishlistRepo;
    private readonly IOrderRepository _orderRepo;

    public GetTemplateDetailHandler(
        ITemplateRepository templateRepo,
        IWishlistRepository wishlistRepo,
        IOrderRepository orderRepo)
    {
        _templateRepo = templateRepo;
        _wishlistRepo = wishlistRepo;
        _orderRepo = orderRepo;
    }

    public async Task<ApiResponse<TemplateDetailDto>> Handle(
        GetTemplateDetailQuery request, CancellationToken cancellationToken)
    {
        var t = await _templateRepo.GetBySlugAsync(request.Slug);
        if (t is null)
            return ApiResponse<TemplateDetailDto>.Fail("Không tìm thấy template");

        if (!request.IsAdmin && t.Status != "Published")
            return ApiResponse<TemplateDetailDto>.Fail("Không tìm thấy template");

        if (!request.IsAdmin)
            await _templateRepo.IncrementViewCountAsync(t.Id);

        var isInWishlist = false;
        var isPurchased = false;

        if (request.CurrentUserId.HasValue && !request.IsAdmin)
        {
            isInWishlist = await _wishlistRepo.ExistsAsync(request.CurrentUserId.Value, t.Id);
            isPurchased = await _orderRepo.HasPurchasedAsync(request.CurrentUserId.Value, t.Id);
        }

        return ApiResponse<TemplateDetailDto>.Ok(
            TemplateMapper.ToDetailDto(t, isInWishlist, isPurchased));
    }
}