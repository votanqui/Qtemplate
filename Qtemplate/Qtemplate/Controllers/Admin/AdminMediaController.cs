using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Media;
using Qtemplate.Application.Features.Media.Commands.DeleteMedia;
using Qtemplate.Application.Features.Media.Commands.LinkMedia;
using Qtemplate.Application.Features.Media.Commands.SetDownloadFile;
using Qtemplate.Application.Features.Media.Commands.UploadMedia;
using Qtemplate.Application.Features.Media.Queries.GetMediaList;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/media")]
[Authorize(Roles = "Admin")]
public class AdminMediaController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminMediaController(IMediator mediator) => _mediator = mediator;

    private string GetAdminId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    // GET /api/admin/media?templateId=&page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] Guid? templateId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetMediaListQuery
        {
            TemplateId = templateId,
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }

    // POST /api/admin/media/upload
    // Body: multipart/form-data { file, templateId? }
    [HttpPost("upload")]
    [RequestSizeLimit(500 * 1024 * 1024)]
    [Consumes("multipart/form-data")]  // ← thêm
    public async Task<IActionResult> Upload([FromForm] UploadMediaRequest request)
    {
        var result = await _mediator.Send(new UploadMediaCommand
        {
            File = request.File,
            TemplateId = request.TemplateId,
            AdminId = GetAdminId()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
    // POST /api/admin/media/link
    // Body: { url, originalName, storageType, externalId?, templateId? }
    [HttpPost("link")]
    public async Task<IActionResult> Link([FromBody] LinkMediaDto dto)
    {
        var result = await _mediator.Send(new LinkMediaCommand
        {
            Url = dto.Url,
            OriginalName = dto.OriginalName,
            StorageType = dto.StorageType,
            ExternalId = dto.ExternalId,
            TemplateId = dto.TemplateId,
            AdminId = GetAdminId()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PUT /api/admin/media/templates/{templateId}/set-download
    // Body: { mediaFileId }
    [HttpPut("templates/{templateId:guid}/set-download")]
    public async Task<IActionResult> SetDownload(Guid templateId, [FromBody] SetDownloadFileDto dto)
    {
        var result = await _mediator.Send(new SetDownloadFileCommand
        {
            TemplateId = templateId,
            MediaFileId = dto.MediaFileId,
            AdminId = GetAdminId()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // DELETE /api/admin/media/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteMediaCommand
        {
            MediaFileId = id,
            AdminId = GetAdminId()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}