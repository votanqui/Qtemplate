using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Affiliates.Commands.PayoutTransaction;

public class PayoutTransactionCommand : IRequest<ApiResponse<bool>>
{
    public int TransactionId { get; set; }
}