using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Media.Commands.SetDownloadFile;

public class SetDownloadFileCommand : IRequest<ApiResponse<object>>
{
    public Guid TemplateId { get; set; }
    public int MediaFileId { get; set; }
    public string AdminId { get; set; } = string.Empty;
}