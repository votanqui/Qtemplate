using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Payments.Commands.SepayCallback;

public class SepayCallbackCommand : IRequest<ApiResponse<object>>
{
    public string SepayId { get; set; } = string.Empty;
    public string? Gateway { get; set; }
    public string? TransactionDate { get; set; }
    public string? AccountNumber { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? TransferType { get; set; }
    public decimal TransferAmount { get; set; }
    public string? ReferenceCode { get; set; }
    public string RawCallback { get; set; } = string.Empty;
}