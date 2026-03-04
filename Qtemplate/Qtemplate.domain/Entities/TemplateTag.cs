namespace Qtemplate.Domain.Entities;

public class TemplateTag
{
    public Guid TemplateId { get; set; }
    public int TagId { get; set; }

    // Navigation
    public Template Template { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}