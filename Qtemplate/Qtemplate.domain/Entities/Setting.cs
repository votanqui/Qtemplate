namespace Qtemplate.Domain.Entities;

public class Setting
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;         // "site_name", "sepay_api_key"
    public string? Value { get; set; }
    public string Group { get; set; } = "General";          // General / Payment / Email
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}