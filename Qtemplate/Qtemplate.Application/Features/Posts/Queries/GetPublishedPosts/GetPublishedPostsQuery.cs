using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Post;

namespace Qtemplate.Application.Features.Posts.Queries.GetPublishedPosts;

public class GetPublishedPostsQuery : IRequest<PaginatedResult<PostListDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string? Search { get; set; }
    public bool? IsFeatured { get; set; }
}
