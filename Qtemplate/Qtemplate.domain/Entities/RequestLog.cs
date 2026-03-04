namespace Qtemplate.Domain.Entities;

public class RequestLog
{
    public long Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public string? UserAgent { get; set; }
    public string? Referer { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}