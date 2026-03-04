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

    [System.Text.Json.Serialization.JsonIgnore]
    public Guid? CurrentUserId { get; set; }
}