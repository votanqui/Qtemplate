using MediatR;
using Microsoft.AspNetCore.Http;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Media;

namespace Qtemplate.Application.Features.Media.Commands.UploadMedia;

public class UploadMediaCommand : IRequest<ApiResponse<MediaFileDto>>
{
    public IFormFile File { get; set; } = null!;
    public Guid? TemplateId { get; set; }
    public string AdminId { get; set; } = string.Empty;
}