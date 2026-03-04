namespace Qtemplate.Domain.Entities;

public class Wishlist
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TemplateId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Template Template { get; set; } = null!;
}