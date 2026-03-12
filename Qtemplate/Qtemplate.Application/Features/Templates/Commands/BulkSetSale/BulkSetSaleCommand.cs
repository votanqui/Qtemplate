using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Templates.Commands.BulkSetSale;

public class BulkSetSaleCommand : IRequest<ApiResponse<object>>
{
    public List<Guid> TemplateIds { get; set; } = new();
    public decimal? SalePrice { get; set; }   // null = xóa sale hàng loạt
    public DateTime? SaleStartAt { get; set; }
    public DateTime? SaleEndAt { get; set; }
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}