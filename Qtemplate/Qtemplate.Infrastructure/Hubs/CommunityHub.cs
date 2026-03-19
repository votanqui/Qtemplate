using Microsoft.AspNetCore.SignalR;

namespace Qtemplate.Infrastructure.Hubs;

public class CommunityHub : Hub
{
    // ── Connection lifecycle ─────────────────────────────────────────────────

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    // ── Client-invokable methods ─────────────────────────────────────────────

    /// <summary>Đăng ký nhận sự kiện bài viết mới trên feed.</summary>
    public async Task JoinFeed()
        => await Groups.AddToGroupAsync(Context.ConnectionId, "community_feed");

    /// <summary>Huỷ đăng ký feed.</summary>
    public async Task LeaveFeed()
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, "community_feed");

    /// <summary>Theo dõi comment của 1 bài viết cụ thể.</summary>
    public async Task JoinPost(int postId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"post_{postId}");

    /// <summary>Rời khỏi group comment của bài viết.</summary>
    public async Task LeavePost(int postId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"post_{postId}");
}