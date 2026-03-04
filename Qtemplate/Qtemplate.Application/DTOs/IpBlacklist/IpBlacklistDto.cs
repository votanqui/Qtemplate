using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.DTOs.IpBlacklist
{
    public class IpBlacklistDto
    {
        public int Id { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? BlockedBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public DateTime BlockedAt { get; set; }
    }

    public class AddIpBlacklistDto
    {
        public string IpAddress { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateTime? ExpiredAt { get; set; }
    }
}
