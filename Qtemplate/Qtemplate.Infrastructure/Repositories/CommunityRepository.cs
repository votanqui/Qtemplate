using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class CommunityRepository : ICommunityRepository
{
    private readonly AppDbContext _db;

    public CommunityRepository(AppDbContext db) => _db = db;


    // POSTS


    public async Task<(List<CommunityPost> Items, int Total)> GetFeedAsync(int page, int pageSize)
    {
        var query = _db.CommunityPosts
            .Include(p => p.User)
            .Where(p => !p.IsHidden)
            .OrderByDescending(p => p.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<CommunityPost?> GetByIdAsync(int id) =>
        await _db.CommunityPosts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task AddPostAsync(CommunityPost post)
    {
        await _db.CommunityPosts.AddAsync(post);
        await _db.SaveChangesAsync();
    }

    public async Task UpdatePostAsync(CommunityPost post)
    {
        _db.CommunityPosts.Update(post);
        await _db.SaveChangesAsync();
    }

    public async Task DeletePostAsync(CommunityPost post)
    {
        _db.CommunityPosts.Remove(post);
        await _db.SaveChangesAsync();
    }


    // LIKES


    public async Task<CommunityLike?> GetLikeAsync(int postId, Guid userId) =>
        await _db.CommunityLikes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

    public async Task AddLikeAsync(CommunityLike like)
    {
        await _db.CommunityLikes.AddAsync(like);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveLikeAsync(CommunityLike like)
    {
        _db.CommunityLikes.Remove(like);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Batch load liked post IDs — dùng 1 query thay vì N queries để kiểm tra từng bài.
    /// </summary>
    public async Task<List<int>> GetLikedPostIdsAsync(IEnumerable<int> postIds, Guid userId) =>
        await _db.CommunityLikes
            .Where(l => postIds.Contains(l.PostId) && l.UserId == userId)
            .Select(l => l.PostId)
            .ToListAsync();


    // COMMENTS


    public async Task<(List<CommunityComment> Items, int Total)> GetCommentsAsync(
        int postId, int page, int pageSize)
    {
        // Chỉ lấy top-level comments (ParentId == null)
        // Replies được load qua Include để tránh thêm query
        var query = _db.CommunityComments
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .Where(c => c.PostId == postId
                     && c.ParentId == null
                     && !c.IsHidden)
            .OrderByDescending(c => c.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<CommunityComment?> GetCommentByIdAsync(int id) =>
        await _db.CommunityComments
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task AddCommentAsync(CommunityComment comment)
    {
        await _db.CommunityComments.AddAsync(comment);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateCommentAsync(CommunityComment comment)
    {
        _db.CommunityComments.Update(comment);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteCommentAsync(CommunityComment comment)
    {
        _db.CommunityComments.Remove(comment);
        await _db.SaveChangesAsync();
    }

  
    // ADMIN
   

    public async Task<(List<CommunityPost> Items, int Total)> GetAdminPostsAsync(
        int page, int pageSize, string? search, bool? isHidden)
    {
        var query = _db.CommunityPosts
            .Include(p => p.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.Content.Contains(search) ||
                p.User.FullName.Contains(search) ||
                p.User.Email.Contains(search));

        if (isHidden.HasValue)
            query = query.Where(p => p.IsHidden == isHidden.Value);

        query = query.OrderByDescending(p => p.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(List<CommunityComment> Items, int Total)> GetAdminCommentsAsync(
        int page, int pageSize, bool? isHidden)
    {
        var query = _db.CommunityComments
            .Include(c => c.User)
            .AsQueryable();

        if (isHidden.HasValue)
            query = query.Where(c => c.IsHidden == isHidden.Value);

        query = query.OrderByDescending(c => c.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}