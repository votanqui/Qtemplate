using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Templates.Commands.ChangeTemplatePricing;

public class ChangeTemplatePricingCommand : IRequest<ApiResponse<object>>
{
    public Guid TemplateId { get; set; }
    public bool IsFree { get; set; }
    public decimal Price { get; set; }  // bắt buộc nếu IsFree = false
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}