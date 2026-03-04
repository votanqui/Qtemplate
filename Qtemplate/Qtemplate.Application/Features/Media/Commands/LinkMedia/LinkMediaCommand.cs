using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Media;

namespace Qtemplate.Application.Features.Media.Commands.LinkMedia;

public class LinkMediaCommand : IRequest<ApiResponse<MediaFileDto>>
{
    public string Url { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string StorageType { get; set; } = "GoogleDrive";
    public string? ExternalId { get; set; }
    public Guid? TemplateId { get; set; }
    public string AdminId { get; set; } = string.Empty;
}