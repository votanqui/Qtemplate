using MediatR;
using Qtemplate.Application.DTOs.Template;
using Qtemplate.Application.DTOs;

public class GetTemplatesQuery : IRequest<ApiResponse<PaginatedResult<TemplateListDto>>>
{
    public string? Search { get; set; }
    public string? CategorySlug { get; set; }
    public string? TagSlug { get; set; }
    public bool? IsFree { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string SortBy { get; set; } = "newest";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;

    // ── Params mới ────────────────────────────────────────────────────────────
    /// <summary>true = chỉ lấy template đang trong sale hợp lệ</summary>
    public bool? OnSale { get; set; }
    /// <summary>true = chỉ lấy template IsFeatured</summary>
    public bool? IsFeatured { get; set; }
    /// <summary>true = chỉ lấy template IsNew</summary>
    public bool? IsNew { get; set; }
    /// <summary>Filter theo tech stack (ví dụ: "React", "Vue")</summary>
    public string? TechStack { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public Guid? CurrentUserId { get; set; }
}