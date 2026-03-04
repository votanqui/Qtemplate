namespace Qtemplate.Domain.Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    // Navigation
    public ICollection<TemplateTag> TemplateTags { get; set; } = new List<TemplateTag>();
}