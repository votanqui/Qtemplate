using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Templates.Commands.SetTemplateSale;

public class SetTemplateSaleCommand : IRequest<ApiResponse<object>>
{
    public Guid TemplateId { get; set; }
    public decimal? SalePrice { get; set; }  // null = xóa sale
    public DateTime? SaleStartAt { get; set; }
    public DateTime? SaleEndAt { get; set; }
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}