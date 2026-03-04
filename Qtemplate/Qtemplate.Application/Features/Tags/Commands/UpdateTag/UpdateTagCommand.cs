using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Tag;

namespace Qtemplate.Application.Features.Tags.Commands.UpdateTag;

public class UpdateTagCommand : IRequest<ApiResponse<object>>
{
    public int Id { get; set; }
    public CreateTagDto Dto { get; set; } = new();
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}