using MediatR;
using Qtemplate.Application.DTOs;

public class DeleteTemplateVersionCommand : IRequest<ApiResponse<object>>
{
    public Guid TemplateId { get; set; } // ← thêm
    public string Version { get; set; } = string.Empty; // ← đổi từ int Id sang string
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}