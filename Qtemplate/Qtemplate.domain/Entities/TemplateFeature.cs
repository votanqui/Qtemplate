namespace Qtemplate.Domain.Entities;

public class TemplateFeature
{
    public int Id { get; set; }
    public Guid TemplateId { get; set; }
    public string Feature { get; set; } = string.Empty;    // "Responsive Design"
    public int SortOrder { get; set; } = 0;

    // Navigation
    public Template Template { get; set; } = null!;
}