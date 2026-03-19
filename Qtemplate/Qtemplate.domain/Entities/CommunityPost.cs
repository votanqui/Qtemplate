using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Domain.Entities
{
    public class CommunityPost
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int LikeCount { get; set; } = 0;
        public int CommentCount { get; set; } = 0;
        public bool IsHidden { get; set; } = false;
        public string? HideReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<CommunityComment> Comments { get; set; } = new List<CommunityComment>();
        public ICollection<CommunityLike> Likes { get; set; } = new List<CommunityLike>();
    }
}
