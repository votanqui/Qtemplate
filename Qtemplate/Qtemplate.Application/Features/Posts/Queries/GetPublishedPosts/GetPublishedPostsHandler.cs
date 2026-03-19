using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Post;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Posts.Queries.GetPublishedPosts;

public class GetPublishedPostsHandler : IRequestHandler<GetPublishedPostsQuery, PaginatedResult<PostListDto>>
{
    private readonly IPostRepository _repo;
    public GetPublishedPostsHandler(IPostRepository repo) => _repo = repo;

    public async Task<PaginatedResult<PostListDto>> Handle(
        GetPublishedPostsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _repo.GetPublishedAsync(
            request.Page, request.PageSize, request.Search, request.IsFeatured);

        return new PaginatedResult<PostListDto>
        {
            Items = items.Select(ToListDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public static PostListDto ToListDto(Post p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Slug = p.Slug,
        Excerpt = p.Excerpt,
        ThumbnailUrl = p.ThumbnailUrl,
        IsFeatured = p.IsFeatured,
        ViewCount = p.ViewCount,
        AuthorName = p.AuthorName,
        Tags = p.Tags,
        PublishedAt = p.PublishedAt,
        CreatedAt = p.CreatedAt
    };

    public static PostDetailDto ToDetailDto(Post p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Slug = p.Slug,
        Excerpt = p.Excerpt,
        Content = p.Content,
        ThumbnailUrl = p.ThumbnailUrl,
        IsFeatured = p.IsFeatured,
        ViewCount = p.ViewCount,
        AuthorName = p.AuthorName,
        Tags = p.Tags,
        MetaTitle = p.MetaTitle,
        MetaDescription = p.MetaDescription,
        PublishedAt = p.PublishedAt,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    public static AdminPostDto ToAdminDto(Post p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Slug = p.Slug,
        Excerpt = p.Excerpt,
        Content = p.Content,
        ThumbnailUrl = p.ThumbnailUrl,
        IsFeatured = p.IsFeatured,
        ViewCount = p.ViewCount,
        AuthorName = p.AuthorName,
        Tags = p.Tags,
        MetaTitle = p.MetaTitle,
        MetaDescription = p.MetaDescription,
        PublishedAt = p.PublishedAt,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        Status = p.Status,
        SortOrder = p.SortOrder
    };
}