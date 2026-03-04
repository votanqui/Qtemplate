using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Analytic.Commands.UpdateTimeOnPage;

public class UpdateTimeOnPageCommand : IRequest<ApiResponse<object>>
{
    public string SessionId { get; set; } = string.Empty;
    public string PageUrl { get; set; } = string.Empty;
    public int Seconds { get; set; }
}