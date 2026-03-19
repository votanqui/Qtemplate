using Qtemplate.Application.DTOs.Community;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Services.Interfaces
{
    public interface ICommunityRealtimeService
    {
        // ── Feed events ──────────────────────────────────────────────────────────
        Task BroadcastNewPostAsync(CommunityPostDto post);
        Task BroadcastPostUpdatedAsync(CommunityPostDto post);
        Task BroadcastPostDeletedAsync(int postId);

        // ── Like event ───────────────────────────────────────────────────────────
        /// <param name="isLikedByActor">true = actor vừa like, false = vừa unlike</param>
        Task BroadcastLikeUpdatedAsync(int postId, int newLikeCount, bool isLikedByActor);

        // ── Comment events (chỉ gửi vào group post_{postId}) ────────────────────
        Task BroadcastNewCommentAsync(CommunityCommentDto comment);
        Task BroadcastCommentUpdatedAsync(CommunityCommentDto comment);
        Task BroadcastCommentDeletedAsync(int commentId, int postId);
    }

}
