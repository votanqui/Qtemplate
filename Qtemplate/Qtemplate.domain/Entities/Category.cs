namespace Qtemplate.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public int? ParentId { get; set; }                      // Hỗ trợ danh mục con
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Template> Templates { get; set; } = new List<Template>();
}