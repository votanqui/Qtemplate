using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Media.Commands.DeleteMedia;

public class DeleteMediaCommand : IRequest<ApiResponse<object>>
{
    public int MediaFileId { get; set; }
    public string AdminId { get; set; } = string.Empty;
}