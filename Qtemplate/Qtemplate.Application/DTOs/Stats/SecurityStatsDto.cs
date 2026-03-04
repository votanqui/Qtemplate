using Qtemplate.Application.DTOs.Email;

namespace Qtemplate.Application.DTOs.Admin;

public class SecurityStatsDto
{
    public IpBlacklistStatsDto IpBlacklist { get; set; } = new();
    public RequestLogStatsDto RequestLogs { get; set; } = new();
    public EmailLogStatsDto EmailLogs { get; set; } = new();
}

public class IpBlacklistStatsDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public int Manual { get; set; }
    public int Auto { get; set; }
    public int Permanent { get; set; }   // ExpiredAt == null
    public int Temporary { get; set; }   // ExpiredAt != null
    public List<RecentBlockedDto> RecentBlocked { get; set; } = new();
}

public class RecentBlockedDto
{
    public string IpAddress { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime BlockedAt { get; set; }
}

public class RequestLogStatsDto
{
    public int TotalRequests { get; set; }
    public int SuccessRequests { get; set; }   // 2xx
    public int ClientErrors { get; set; }   // 4xx
    public int ServerErrors { get; set; }   // 5xx
    public double SuccessRate { get; set; }   // %
    public double ErrorRate { get; set; }   // %
    public long AvgResponseTime { get; set; }   // ms
    public long MaxResponseTime { get; set; }   // ms
    public List<EndpointStatDto> TopEndpoints { get; set; } = new();
    public List<StatusCodeStatDto> ByStatusCode { get; set; } = new();
    public List<IpRequestStatDto> TopIps { get; set; } = new();
}

public class EndpointStatDto
{
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int Count { get; set; }
    public long AvgResponseTime { get; set; }
}

public class StatusCodeStatDto
{
    public int StatusCode { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class IpRequestStatDto
{
    public string IpAddress { get; set; } = string.Empty;
    public int Count { get; set; }
    public int ErrorCount { get; set; }
}

public class EmailLogStatsDto
{
    public int Total { get; set; }
    public int Sent { get; set; }
    public int Failed { get; set; }
    public int Pending { get; set; }
    public double SuccessRate { get; set; }   // %
    public double FailureRate { get; set; }   // %
    public List<EmailTemplateStatDto> ByTemplate { get; set; } = new();
    public List<EmailLogDto> RecentFailed { get; set; } = new();
}

public class EmailTemplateStatDto
{
    public string Template { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Sent { get; set; }
    public int Failed { get; set; }
}
public class RefreshTokenStatsDto
{
    public int Total { get; set; }
    public int Active { get; set; }   // chưa revoke + chưa expire
    public int Revoked { get; set; }
    public int Expired { get; set; }   // chưa revoke nhưng đã expire
    public int RevokedByAdmin { get; set; }   // RevokedReason = "AdminLocked" / "AccountDeleted"
    public int RevokedByLogout { get; set; }   // RevokedReason = "Logout"
    public List<SuspiciousTokenDto> SuspiciousIps { get; set; } = new();  // 1 user nhiều IP
}
public class SuspiciousTokenDto
{
    public Guid UserId { get; set; }
    public string? UserEmail { get; set; }
    public int IpCount { get; set; }   // số IP khác nhau
    public List<string> IpAddresses { get; set; } = new();
}