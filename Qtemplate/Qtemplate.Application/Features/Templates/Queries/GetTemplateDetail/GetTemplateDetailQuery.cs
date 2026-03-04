using MediatR;
using Qtemplate.Application.DTOs.Template;
using Qtemplate.Application.DTOs;

public class GetTemplateDetailQuery : IRequest<ApiResponse<TemplateDetailDto>>
{
    public string Slug { get; set; } = string.Empty;
    public Guid? CurrentUserId { get; set; }
    public bool IsAdmin { get; set; } = false; // ← thêm
}