using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Application.Features.Auth.Commands.ChangePassword;
using Qtemplate.Application.Features.Auth.Commands.ForgotPassword;
using Qtemplate.Application.Features.Auth.Commands.Login;
using Qtemplate.Application.Features.Auth.Commands.Logout;
using Qtemplate.Application.Features.Auth.Commands.RenewToken;
using Qtemplate.Application.Features.Auth.Commands.Register;
using Qtemplate.Application.Features.Auth.Commands.ResendVerifyEmail;
using Qtemplate.Application.Features.Auth.Commands.ResetPassword;
using Qtemplate.Application.Features.Auth.Commands.VerifyEmail;

namespace Qtemplate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string GetIpAddress() =>
        Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"].ToString()
            : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private string GetUserAgent() =>
        Request.Headers.UserAgent.ToString();

    private void SetAccessTokenCookie(string accessToken, DateTime expiry)
    {
        Response.Cookies.Append("accessToken", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = expiry
        });
    }

    private void ClearAccessTokenCookie()
    {
        Response.Cookies.Delete("accessToken");
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _mediator.Send(new LoginCommand
        {
            Email = dto.Email,
            Password = dto.Password,
            IpAddress = GetIpAddress(),
            UserAgent = GetUserAgent()
        });

        if (!result.Success) return BadRequest(result);

        SetAccessTokenCookie(result.Data!.AccessToken, result.Data.AccessTokenExpiry);
        result.Data.AccessToken = string.Empty;
        return Ok(result);
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _mediator.Send(new RegisterCommand
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Password = dto.Password,
            ConfirmPassword = dto.ConfirmPassword,
            IpAddress = GetIpAddress(),
            UserAgent = GetUserAgent()
        });

        if (!result.Success) return BadRequest(result);

        SetAccessTokenCookie(result.Data!.AccessToken, result.Data.AccessTokenExpiry);
        result.Data.AccessToken = string.Empty;
        return Ok(result);
    }

    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        if (string.IsNullOrEmpty(dto.RefreshToken))
            return Unauthorized(new { success = false, message = "Không tìm thấy refresh token" });

        var result = await _mediator.Send(new RenewTokenCommand
        {
            RefreshToken = dto.RefreshToken,
            IpAddress = GetIpAddress(),
            UserAgent = GetUserAgent()
        });

        if (!result.Success) return Unauthorized(result);

        SetAccessTokenCookie(result.Data!.AccessToken, result.Data.AccessTokenExpiry);
        result.Data.AccessToken = string.Empty;
        return Ok(result);
    }

    [HttpPost("Logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto)
    {
        if (!string.IsNullOrEmpty(dto.RefreshToken))
            await _mediator.Send(new LogoutCommand { RefreshToken = dto.RefreshToken });

        ClearAccessTokenCookie();
        return Ok(new { success = true, message = "Đăng xuất thành công" });
    }

    [HttpPost("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var result = await _mediator.Send(new ForgotPasswordCommand { Email = dto.Email });
        return Ok(result);
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var result = await _mediator.Send(new ResetPasswordCommand
        {
            Token = dto.Token,
            NewPassword = dto.NewPassword,
            ConfirmPassword = dto.ConfirmPassword
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpPost("ChangePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized(new { success = false, message = "Token không hợp lệ" });

        var result = await _mediator.Send(new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = dto.CurrentPassword,
            NewPassword = dto.NewPassword,
            ConfirmPassword = dto.ConfirmPassword
        });

        if (result.Success) ClearAccessTokenCookie();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("VerifyEmail")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        var result = await _mediator.Send(new VerifyEmailCommand { Token = token });
        return result.Success ? Ok(result) : BadRequest(result);
    }

   [HttpPost("ResendVerifyEmail")]
    public async Task<IActionResult> ResendVerifyEmail([FromBody] ResendVerifyEmailDto dto)
    {
        var result = await _mediator.Send(new ResendVerifyEmailCommand { Email = dto.Email });
        return Ok(result);
    }
}