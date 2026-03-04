using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Order;

namespace Qtemplate.Application.Features.Payments.Commands.CreatePayment;

public class CreatePaymentCommand : IRequest<ApiResponse<CreatePaymentResultDto>>
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
}