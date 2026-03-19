using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Domain.Entities
{
    public class CommunityComment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public Guid UserId { get; set; }
        public int? ParentId { get; set; }              // null = top-level, có giá trị = reply (1 cấp)
        public string Content { get; set; } = string.Empty;
        public bool IsHidden { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public CommunityPost Post { get; set; } = null!;
        public User User { get; set; } = null!;
        public CommunityComment? Parent { get; set; }
        public ICollection<CommunityComment> Replies { get; set; } = new List<CommunityComment>();
    }
}
