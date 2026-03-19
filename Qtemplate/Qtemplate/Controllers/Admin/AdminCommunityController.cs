// Qtemplate/Controllers/Admin/AdminCommunityController.cs

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Community;
using Qtemplate.Application.Features.Community.Commands.AdminDeleteComment;
using Qtemplate.Application.Features.Community.Commands.AdminDeletePost;
using Qtemplate.Application.Features.Community.Commands.HideComment;
using Qtemplate.Application.Features.Community.Commands.HidePost;
using Qtemplate.Application.Features.Community.Queries.AdminGetComments;
using Qtemplate.Application.Features.Community.Queries.AdminGetPosts;
using Qtemplate.Application.Features.Community.Queries.AdminGetPostsCommunity;
using Qtemplate.Application.Features.Posts.Queries.AdminGetPosts;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/community")]
[Authorize(Roles = "Admin")]
public class AdminCommunityController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminCommunityController(IMediator mediator) => _mediator = mediator;

    // ── GET /api/admin/community/posts ────────────────────────────────────────
    /// <summary>Lấy toàn bộ bài viết (bao gồm ẩn). Hỗ trợ tìm kiếm và lọc theo IsHidden.</summary>
    [HttpGet("posts")]
    public async Task<IActionResult> GetPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isHidden = null)
    {
        var result = await _mediator.Send(new AdminGetPostsCommunityQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            IsHidden = isHidden,
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── PATCH /api/admin/community/posts/{id}/hide ────────────────────────────
    /// <summary>Ẩn hoặc hiện lại bài viết. Body: { isHidden, reason? }.</summary>
    [HttpPatch("posts/{id:int}/hide")]
    public async Task<IActionResult> HidePost(int id, [FromBody] HideContentDto dto)
    {
        var result = await _mediator.Send(new HidePostCommand
        {
            PostId = id,
            IsHidden = dto.IsHidden,
            Reason = dto.Reason,
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── DELETE /api/admin/community/posts/{id} ────────────────────────────────
    /// <summary>Admin xóa vĩnh viễn bài viết (bao gồm ảnh đính kèm).</summary>
    [HttpDelete("posts/{id:int}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        var result = await _mediator.Send(new AdminDeletePostCommand { PostId = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── GET /api/admin/community/comments ─────────────────────────────────────
    /// <summary>Lấy toàn bộ bình luận (bao gồm ẩn). Hỗ trợ lọc theo IsHidden.</summary>
    [HttpGet("comments")]
    public async Task<IActionResult> GetComments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isHidden = null)
    {
        var result = await _mediator.Send(new AdminGetCommentsQuery
        {
            Page = page,
            PageSize = pageSize,
            IsHidden = isHidden,
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── PATCH /api/admin/community/comments/{id}/hide ─────────────────────────
    /// <summary>Ẩn hoặc hiện lại bình luận. Body: { isHidden }.</summary>
    [HttpPatch("comments/{id:int}/hide")]
    public async Task<IActionResult> HideComment(int id, [FromBody] HideContentDto dto)
    {
        var result = await _mediator.Send(new HideCommentCommand
        {
            CommentId = id,
            IsHidden = dto.IsHidden,
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── DELETE /api/admin/community/comments/{id} ─────────────────────────────
    /// <summary>Admin xóa vĩnh viễn bình luận (tự động giảm CommentCount trên post).</summary>
    [HttpDelete("comments/{id:int}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var result = await _mediator.Send(new AdminDeleteCommentCommand { CommentId = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}