// DTOs/Template/Admin/TemplateVersionDto.cs
namespace Qtemplate.Application.DTOs.Template.Admin;

public class TemplateVersionDto
{
    public int Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public string? ChangeLog { get; set; }
    public bool IsLatest { get; set; }
    public DateTime CreatedAt { get; set; }
}