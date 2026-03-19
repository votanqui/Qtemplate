// Qtemplate/Controllers/Admin/AdminPostController.cs

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Post;
using Qtemplate.Application.Features.Posts.Commands.CreatePost;
using Qtemplate.Application.Features.Posts.Commands.DeletePost;
using Qtemplate.Application.Features.Posts.Commands.UpdatePost;
using Qtemplate.Application.Features.Posts.Queries.AdminGetPosts;
using Qtemplate.Application.Services.Interfaces;
using System.Security.Claims;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/posts")]
[Authorize(Roles = "Admin")]
public class AdminPostController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileUploadService _fileUploadService;

    public AdminPostController(IMediator mediator, IFileUploadService fileUploadService)
    {
        _mediator = mediator;
        _fileUploadService = fileUploadService;
    }

    /// <summary>
    /// GET /api/admin/posts
    /// Danh sách tất cả bài viết. Lọc theo status (Draft/Published/Archived) và tìm kiếm.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        var result = await _mediator.Send(new AdminGetPostsQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            Status = status
        });
        return Ok(result);
    }

    /// <summary>
    /// POST /api/admin/posts
    /// Tạo bài viết mới. Upload thumbnail qua multipart/form-data (field: thumbnailFile).
    /// Các field text truyền qua [FromQuery].
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Create(
        IFormFile? thumbnailFile,
        [FromQuery] string title = "",
        [FromQuery] string? slug = null,
        [FromQuery] string? excerpt = null,
        [FromQuery] string content = "",
        [FromQuery] string? thumbnailUrl = null,
        [FromQuery] string status = "Draft",
        [FromQuery] bool isFeatured = false,
        [FromQuery] int sortOrder = 0,
        [FromQuery] string? tags = null,
        [FromQuery] string? metaTitle = null,
        [FromQuery] string? metaDescription = null,
        [FromQuery] DateTime? publishedAt = null)
    {
        var finalThumbnailUrl = await ResolvePostImageAsync(thumbnailFile, thumbnailUrl);

        var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var authorName = User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("username")
            ?? "Admin";

        var result = await _mediator.Send(new CreatePostCommand
        {
            AuthorId = authorId,
            AuthorName = authorName,
            Data = BuildDto(title, slug, excerpt, content, finalThumbnailUrl,
                status, isFeatured, sortOrder, tags, metaTitle, metaDescription, publishedAt)
        });

        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// PUT /api/admin/posts/{id}
    /// Cập nhật bài viết. Upload thumbnail mới hoặc giữ URL cũ qua thumbnailUrl query param.
    /// </summary>
    [HttpPut("{id:int}")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Update(
        int id,
        IFormFile? thumbnailFile,
        [FromQuery] string title = "",
        [FromQuery] string? slug = null,
        [FromQuery] string? excerpt = null,
        [FromQuery] string content = "",
        [FromQuery] string? thumbnailUrl = null,
        [FromQuery] string status = "Draft",
        [FromQuery] bool isFeatured = false,
        [FromQuery] int sortOrder = 0,
        [FromQuery] string? tags = null,
        [FromQuery] string? metaTitle = null,
        [FromQuery] string? metaDescription = null,
        [FromQuery] DateTime? publishedAt = null)
    {
        var finalThumbnailUrl = await ResolvePostImageAsync(thumbnailFile, thumbnailUrl);

        var result = await _mediator.Send(new UpdatePostCommand
        {
            Id = id,
            Data = BuildDto(title, slug, excerpt, content, finalThumbnailUrl,
                status, isFeatured, sortOrder, tags, metaTitle, metaDescription, publishedAt)
        });

        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// DELETE /api/admin/posts/{id}
    /// Xóa bài viết vĩnh viễn.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeletePostCommand { Id = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Nếu có file upload → lưu vào wwwroot/post-images/ (thư mục riêng cho bài viết).
    /// Nếu không có file → giữ nguyên URL cũ truyền vào.
    /// </summary>
    private async Task<string?> ResolvePostImageAsync(IFormFile? file, string? existingUrl)
    {
        if (file is not { Length: > 0 }) return existingUrl;

        try
        {
            await using var stream = file.OpenReadStream();
            // Dùng SavePostImageAsync → lưu vào wwwroot/post-images/ (không dùng chung với banner)
            return await _fileUploadService.SavePostImageAsync(stream, file.FileName, file.Length);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(ex.Message);
        }
    }

    private static UpsertPostDto BuildDto(
        string title, string? slug, string? excerpt, string content,
        string? thumbnailUrl, string status, bool isFeatured, int sortOrder,
        string? tags, string? metaTitle, string? metaDescription,
        DateTime? publishedAt) => new()
        {
            Title = title,
            Slug = slug,
            Excerpt = excerpt,
            Content = content,
            ThumbnailUrl = thumbnailUrl,
            Status = status,
            IsFeatured = isFeatured,
            SortOrder = sortOrder,
            Tags = tags,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            PublishedAt = publishedAt
        };
}