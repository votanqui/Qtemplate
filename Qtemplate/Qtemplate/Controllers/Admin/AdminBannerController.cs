// Controllers/Admin/AdminBannerController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Banner;
using Qtemplate.Application.Features.Banners.Commands.CreateBanne;
using Qtemplate.Application.Features.Banners.Commands.DeleteBanner;
using Qtemplate.Application.Features.Banners.Commands.UpdateBanner;
using Qtemplate.Application.Features.Banners.Queries.AdminGetBanner;
using Qtemplate.Application.Services.Interfaces;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/banners")]
[Authorize(Roles = "Admin")]
public class AdminBannerController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileUploadService _fileUploadService;

    public AdminBannerController(IMediator mediator, IFileUploadService fileUploadService)
    {
        _mediator = mediator;
        _fileUploadService = fileUploadService;
    }

    // GET /api/admin/banners
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAdminBannersQuery { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    // POST /api/admin/banners
    // - imageFile (IFormFile, tuỳ chọn): upload ảnh mới
    // - Các field khác qua [FromQuery]
    // Nếu không upload ảnh thì truyền imageUrl qua query để giữ URL cũ
    [HttpPost]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> Create(
        IFormFile? imageFile,
        [FromQuery] string title = "",
        [FromQuery] string? subTitle = null,
        [FromQuery] string? imageUrl = null,
        [FromQuery] string? linkUrl = null,
        [FromQuery] string position = "Home",
        [FromQuery] int sortOrder = 0,
        [FromQuery] bool isActive = true,
        [FromQuery] DateTime? startAt = null,
        [FromQuery] DateTime? endAt = null)
    {
        var finalImageUrl = await ResolveImageUrlAsync(imageFile, imageUrl);
        if (finalImageUrl == null && imageFile != null)
            return BadRequest(new { message = "Upload ảnh thất bại" });

        var dto = BuildDto(title, subTitle, finalImageUrl ?? string.Empty,
            linkUrl, position, sortOrder, isActive, startAt, endAt);

        var result = await _mediator.Send(new CreateBannerCommand { Data = dto });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PUT /api/admin/banners/{id}
    [HttpPut("{id:int}")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> Update(
        int id,
        IFormFile? imageFile,
        [FromQuery] string title = "",
        [FromQuery] string? subTitle = null,
        [FromQuery] string? imageUrl = null,
        [FromQuery] string? linkUrl = null,
        [FromQuery] string position = "Home",
        [FromQuery] int sortOrder = 0,
        [FromQuery] bool isActive = true,
        [FromQuery] DateTime? startAt = null,
        [FromQuery] DateTime? endAt = null)
    {
        var finalImageUrl = await ResolveImageUrlAsync(imageFile, imageUrl);

        var dto = BuildDto(title, subTitle, finalImageUrl ?? string.Empty,
            linkUrl, position, sortOrder, isActive, startAt, endAt);

        var result = await _mediator.Send(new UpdateBannerCommand { Id = id, Data = dto });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // DELETE /api/admin/banners/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteBannerCommand { Id = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string?> ResolveImageUrlAsync(IFormFile? imageFile, string? existingUrl)
    {
        if (imageFile is not { Length: > 0 }) return existingUrl;
        try
        {
            await using var stream = imageFile.OpenReadStream();
            return await _fileUploadService.SaveBannerImageAsync(
                stream, imageFile.FileName, imageFile.Length);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(ex.Message);
        }
    }

    private static UpsertBannerDto BuildDto(
        string title, string? subTitle, string imageUrl,
        string? linkUrl, string position, int sortOrder,
        bool isActive, DateTime? startAt, DateTime? endAt) => new()
        {
            Title = title,
            SubTitle = subTitle,
            ImageUrl = imageUrl,
            LinkUrl = linkUrl,
            Position = position,
            SortOrder = sortOrder,
            IsActive = isActive,
            StartAt = startAt,
            EndAt = endAt,
        };
}