using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Template;

namespace Qtemplate.Application.Features.Templates.Queries.GetOnSaleTemplates;

/// <summary>
/// Query riêng cho trang "Săn Sale" — chỉ trả về templates đang sale hợp lệ,
/// kèm thông tin countdown (SaleEndAt), sắp xếp theo % giảm cao nhất.
/// </summary>
public class GetOnSaleTemplatesQuery : IRequest<ApiResponse<PaginatedResult<SaleTemplateDto>>>
{
    public string? Search { get; set; }
    public string? CategorySlug { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;

    [System.Text.Json.Serialization.JsonIgnore]
    public Guid? CurrentUserId { get; set; }
}