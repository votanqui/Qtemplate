using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.DTOs.RefreshToken
{
    public class RefreshTokenDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string? UserEmail { get; set; }
        public string Token { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsExpired { get; set; }
        public bool IsActive { get; set; }
        public string? RevokedReason { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
