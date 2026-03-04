namespace Qtemplate.Domain.Entities;

public class TemplateImage
{
    public int Id { get; set; }
    public Guid TemplateId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public string Type { get; set; } = "Screenshot";       // Screenshot / Thumbnail / Banner
    public int SortOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Template Template { get; set; } = null!;
}