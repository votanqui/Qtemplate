using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Community;
using Qtemplate.Application.Features.Community.Commands.CreateComment;
using Qtemplate.Application.Features.Community.Commands.CreatePost;
using Qtemplate.Application.Features.Community.Commands.DeleteComment;
using Qtemplate.Application.Features.Community.Commands.DeletePost;
using Qtemplate.Application.Features.Community.Commands.ToggleLike;
using Qtemplate.Application.Features.Community.Commands.UpdateComment;
using Qtemplate.Application.Features.Community.Commands.UpdatePost;
using Qtemplate.Application.Features.Community.Queries.GetComments;
using Qtemplate.Application.Features.Community.Queries.GetFeed;
using Qtemplate.Application.Services.Interfaces;

namespace Qtemplate.Controllers;

[ApiController]
[Route("api/community")]
public class CommunityController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileUploadService _fileUploadService;

    public CommunityController(IMediator mediator, IFileUploadService fileUploadService)
    {
        _mediator = mediator;
        _fileUploadService = fileUploadService;
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    // ── GET /api/community/feed?page=&pageSize= ───────────────────────────────
    /// <summary>Lấy feed bài viết cộng đồng (public, phân trang).</summary>
    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetFeedQuery
        {
            Page = page,
            PageSize = pageSize,
            CurrentUserId = GetUserId(),
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── POST /api/community/posts ─────────────────────────────────────────────
    /// <summary>
    /// Đăng bài mới. Ảnh upload qua multipart (field: imageFile).
    /// Các field text qua [FromQuery].
    /// </summary>
    [HttpPost("posts")]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> CreatePost(
        IFormFile? imageFile,
        [FromQuery] string content = "",
        [FromQuery] string? imageUrl = null)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var finalImageUrl = await ResolveImageAsync(imageFile, imageUrl);

        var result = await _mediator.Send(new CreatePostCommand
        {
            UserId = userId.Value,
            Content = content,
            ImageUrl = finalImageUrl,
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── PUT /api/community/posts/{id} ─────────────────────────────────────────
    /// <summary>Cập nhật bài viết. Chỉ tác giả mới được sửa.</summary>
    [HttpPut("posts/{id:int}")]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UpdatePost(
        int id,
        IFormFile? imageFile,
        [FromQuery] string content = "",
        [FromQuery] string? imageUrl = null)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var finalImageUrl = await ResolveImageAsync(imageFile, imageUrl);

        var result = await _mediator.Send(new UpdatePostCommand
        {
            PostId = id,
            UserId = userId.Value,
            Content = content,
            ImageUrl = finalImageUrl,
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── DELETE /api/community/posts/{id} ──────────────────────────────────────
    /// <summary>Xóa bài viết. Chỉ tác giả mới được xóa.</summary>
    [HttpDelete("posts/{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new DeletePostCommand
        {
            PostId = id,
            UserId = userId.Value,
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── POST /api/community/posts/{id}/like ───────────────────────────────────
    /// <summary>Toggle like/unlike bài viết. Trả về true = đang like, false = đã bỏ like.</summary>
    [HttpPost("posts/{id:int}/like")]
    [Authorize]
    public async Task<IActionResult> ToggleLike(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new ToggleLikeCommand
        {
            PostId = id,
            UserId = userId.Value,
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── GET /api/community/posts/{id}/comments?page=&pageSize= ───────────────
    /// <summary>Lấy danh sách comments của bài viết (top-level, kèm replies).</summary>
    [HttpGet("posts/{id:int}/comments")]
    public async Task<IActionResult> GetComments(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetCommentsQuery
        {
            PostId = id,
            Page = page,
            PageSize = pageSize,
            CurrentUserId = GetUserId(),
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── POST /api/community/posts/{id}/comments ───────────────────────────────
    /// <summary>Đăng bình luận hoặc reply. Truyền parentId để reply.</summary>
    [HttpPost("posts/{id:int}/comments")]
    [Authorize]
    public async Task<IActionResult> CreateComment(
        int id,
        [FromBody] CreateCommentDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new CreateCommentCommand
        {
            PostId = id,
            UserId = userId.Value,
            Content = dto.Content,
            ParentId = dto.ParentId,
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── PUT /api/community/comments/{id} ──────────────────────────────────────
    /// <summary>Cập nhật nội dung comment. Chỉ tác giả mới được sửa.</summary>
    [HttpPut("comments/{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateComment(
        int id,
        [FromBody] UpdateCommentDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new UpdateCommentCommand
        {
            CommentId = id,
            UserId = userId.Value,
            Content = dto.Content,
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── DELETE /api/community/comments/{id} ───────────────────────────────────
    /// <summary>Xóa comment. Chỉ tác giả mới được xóa.</summary>
    [HttpDelete("comments/{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new DeleteCommentCommand
        {
            CommentId = id,
            UserId = userId.Value,
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Helper ────────────────────────────────────────────────────────────────
    private async Task<string?> ResolveImageAsync(IFormFile? file, string? existingUrl)
    {
        if (file is not { Length: > 0 }) return existingUrl;
        await using var stream = file.OpenReadStream();
        return await _fileUploadService.SavePostImageAsync(stream, file.FileName, file.Length);
    }
}