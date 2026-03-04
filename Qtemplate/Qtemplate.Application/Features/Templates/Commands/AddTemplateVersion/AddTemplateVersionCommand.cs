using MediatR;
using Microsoft.AspNetCore.Http;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Templates.Commands.AddTemplateVersion;

public class AddTemplateVersionCommand : IRequest<ApiResponse<int>>
{
    public Guid TemplateId { get; set; }
    public string Version { get; set; } = string.Empty;
    public string? ChangeLog { get; set; }
    public IFormFile? File { get; set; }          // null nếu dùng external
    // External
    public string? ExternalUrl { get; set; }          // GDrive/S3 URL
    public string StorageType { get; set; } = "Local";
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}