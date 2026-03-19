using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface ICommunityRepository
{
    // ── Posts ─────────────────────────────────────────────────────────────────
    Task<(List<CommunityPost> Items, int Total)> GetFeedAsync(int page, int pageSize);
    Task<CommunityPost?> GetByIdAsync(int id);
    Task AddPostAsync(CommunityPost post);
    Task UpdatePostAsync(CommunityPost post);
    Task DeletePostAsync(CommunityPost post);

    // ── Likes ─────────────────────────────────────────────────────────────────
    Task<CommunityLike?> GetLikeAsync(int postId, Guid userId);
    Task AddLikeAsync(CommunityLike like);
    Task RemoveLikeAsync(CommunityLike like);
    Task<List<int>> GetLikedPostIdsAsync(IEnumerable<int> postIds, Guid userId);

    // ── Comments ──────────────────────────────────────────────────────────────
    Task<(List<CommunityComment> Items, int Total)> GetCommentsAsync(int postId, int page, int pageSize);
    Task<CommunityComment?> GetCommentByIdAsync(int id);
    Task AddCommentAsync(CommunityComment comment);
    Task UpdateCommentAsync(CommunityComment comment);
    Task DeleteCommentAsync(CommunityComment comment);

    // ── Admin ─────────────────────────────────────────────────────────────────
    Task<(List<CommunityPost> Items, int Total)> GetAdminPostsAsync(
        int page, int pageSize, string? search, bool? isHidden);
    Task<(List<CommunityComment> Items, int Total)> GetAdminCommentsAsync(
        int page, int pageSize, bool? isHidden);
}