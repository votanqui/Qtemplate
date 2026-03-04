namespace Qtemplate.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty; // Snapshot tên lúc mua
    public decimal OriginalPrice { get; set; }               // Giá gốc
    public decimal Price { get; set; }                       // Giá thực tế đã mua
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Order Order { get; set; } = null!;
    public Template Template { get; set; } = null!;
}