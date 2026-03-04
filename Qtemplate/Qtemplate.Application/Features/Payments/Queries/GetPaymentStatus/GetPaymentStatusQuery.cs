using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.payments;

namespace Qtemplate.Application.Features.Payments.Queries.GetPaymentStatus;

public class GetPaymentStatusQuery : IRequest<ApiResponse<PaymentStatusDto>>
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
}

