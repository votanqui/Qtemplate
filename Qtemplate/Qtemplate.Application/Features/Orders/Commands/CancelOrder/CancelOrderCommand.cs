using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Orders.Commands.CancelOrder;

public class CancelOrderCommand : IRequest<ApiResponse<object>>
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public bool IsAdmin { get; set; } = false;
    public string? CancelReason { get; set; }
}