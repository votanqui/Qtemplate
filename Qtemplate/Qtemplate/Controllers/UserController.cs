using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.User;

using Qtemplate.Application.Features.UserManagement.Commands.DeleteAccount;
using Qtemplate.Application.Features.UserManagement.Commands.UpdateAvatar;
using Qtemplate.Application.Features.UserManagement.Queries.DownloadHistoryQuery;
using Qtemplate.Application.Features.UserManagement.Queries.GetProfile;
using Qtemplate.Application.Features.UserManagement.Commands.MarkNotificationRead;
using Qtemplate.Application.Features.UserManagement.Queries.Notifications;
using Qtemplate.Application.Features.UserManagement.Queries.PurchaseHistory;
using Qtemplate.Application.Features.UserManagement.Commands.ToggleWishlists;
using Qtemplate.Application.Features.UserManagement.Queries.Wishlist;

namespace Qtemplate.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetUserId()
    {
        var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }

    private string GetIpAddress() =>
        Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"].ToString()
            : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    // ──────────────────────────────────────────────
    // PROFILE
    // ──────────────────────────────────────────────

    /// <summary>Lấy thông tin profile của user đang đăng nhập</summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetProfileQuery { UserId = userId });
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Cập nhật thông tin profile</summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new UpdateProfileCommand
        {
            UserId = userId,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            IpAddress = GetIpAddress()
        });

        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Cập nhật avatar</summary>
    [HttpPut("avatar")]
    [RequestSizeLimit(2 * 1024 * 1024)] // Giới hạn 2MB ở tầng HTTP
    public async Task<IActionResult> UpdateAvatar(IFormFile file)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new UpdateAvatarCommand
        {
            UserId = userId,
            File = file,
            IpAddress = GetIpAddress()
        });

        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Xóa tài khoản (soft delete / deactivate)</summary>
    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new DeleteAccountCommand
        {
            UserId = userId,
            Password = dto.Password,
            IpAddress = GetIpAddress()
        });

        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ──────────────────────────────────────────────
    // PURCHASE HISTORY
    // ──────────────────────────────────────────────

    /// <summary>Lịch sử mua hàng</summary>
    [HttpGet("purchases")]
    public async Task<IActionResult> GetPurchaseHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetPurchaseHistoryQuery
        {
            UserId = userId,
            Page = page,
            PageSize = pageSize,
            Status = status
        });

        return Ok(result);
    }

    // ──────────────────────────────────────────────
    // DOWNLOAD HISTORY
    // ──────────────────────────────────────────────

    /// <summary>Lịch sử download</summary>
    [HttpGet("downloads")]
    public async Task<IActionResult> GetDownloadHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetDownloadHistoryQuery
        {
            UserId = userId,
            Page = page,
            PageSize = pageSize
        });

        return Ok(result);
    }

    // ──────────────────────────────────────────────
    // WISHLIST
    // ──────────────────────────────────────────────

    /// <summary>Danh sách yêu thích</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyWishlist([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var result = await _mediator.Send(new GetWishlistQuery
        {
            UserId = GetUserId(),
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }
    [HttpPost("{templateId:guid}")]
    public async Task<IActionResult> Toggle(Guid templateId)
    {
        var result = await _mediator.Send(new ToggleWishlistCommand
        {
            UserId = GetUserId(),
            TemplateId = templateId
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
    /// <summary>Thêm / bỏ yêu thích template (toggle)</summary>
    [HttpPost("wishlist/{templateId:guid}")]
    public async Task<IActionResult> ToggleWishlist(Guid templateId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new ToggleWishlistCommand
        {
            UserId = userId,
            TemplateId = templateId
        });

        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ──────────────────────────────────────────────
    // NOTIFICATIONS
    // ──────────────────────────────────────────────

    /// <summary>Danh sách thông báo</summary>
    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? unreadOnly = null)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetNotificationsQuery
        {
            UserId = userId,
            Page = page,
            PageSize = pageSize,
            UnreadOnly = unreadOnly
        });

        return Ok(result);
    }

    /// <summary>Đánh dấu thông báo đã đọc</summary>
    [HttpPatch("notifications/{notificationId:int}/read")]
    public async Task<IActionResult> MarkNotificationRead(int notificationId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new MarkNotificationReadCommand
        {
            UserId = userId,
            NotificationId = notificationId
        });

        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Đánh dấu tất cả thông báo đã đọc</summary>
    [HttpPatch("notifications/read-all")]
    public async Task<IActionResult> MarkAllNotificationsRead()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new MarkNotificationReadCommand
        {
            UserId = userId,
            NotificationId = null // null = mark all
        });

        return result.Success ? Ok(result) : BadRequest(result);
    }
}