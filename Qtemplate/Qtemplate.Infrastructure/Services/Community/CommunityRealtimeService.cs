using Microsoft.AspNetCore.SignalR;
using Qtemplate.Application.DTOs.Community;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Infrastructure.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Infrastructure.Services.Community
{
    public class CommunityRealtimeService : ICommunityRealtimeService
    {
        private readonly IHubContext<CommunityHub> _hub;

        public CommunityRealtimeService(IHubContext<CommunityHub> hub)
            => _hub = hub;

        // ── Feed ─────────────────────────────────────────────────────────────────

        public Task BroadcastNewPostAsync(CommunityPostDto post)
            => _hub.Clients.Group("community_feed")
                   .SendAsync("NewPost", post);

        public Task BroadcastPostUpdatedAsync(CommunityPostDto post)
            => _hub.Clients.Group("community_feed")
                   .SendAsync("PostUpdated", post);

        public Task BroadcastPostDeletedAsync(int postId)
            => _hub.Clients.Group("community_feed")
                   .SendAsync("PostDeleted", postId);

        // ── Like ─────────────────────────────────────────────────────────────────

        public Task BroadcastLikeUpdatedAsync(int postId, int newLikeCount, bool isLikedByActor)
            => _hub.Clients.Group("community_feed")
                   .SendAsync("LikeUpdated", postId, newLikeCount, isLikedByActor);

        // ── Comments ─────────────────────────────────────────────────────────────

        public Task BroadcastNewCommentAsync(CommunityCommentDto comment)
            => _hub.Clients.Group($"post_{comment.PostId}")
                   .SendAsync("NewComment", comment);

        public Task BroadcastCommentUpdatedAsync(CommunityCommentDto comment)
            => _hub.Clients.Group($"post_{comment.PostId}")
                   .SendAsync("CommentUpdated", comment);

        public Task BroadcastCommentDeletedAsync(int commentId, int postId)
            => _hub.Clients.Group($"post_{postId}")
                   .SendAsync("CommentDeleted", commentId, postId);
    }
}
