using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Tags.Commands.DeleteTag;

public class DeleteTagCommand : IRequest<ApiResponse<object>>
{
    public int Id { get; set; }
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}