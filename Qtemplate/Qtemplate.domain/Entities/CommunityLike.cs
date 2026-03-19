using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Domain.Entities
{
    public class CommunityLike
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public CommunityPost Post { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
