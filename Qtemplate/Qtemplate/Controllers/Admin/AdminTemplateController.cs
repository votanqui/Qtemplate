using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Template.Admin;
using Qtemplate.Application.Features.Templates.Commands.AddTemplateImage;
using Qtemplate.Application.Features.Templates.Commands.AddTemplateVersion;
using Qtemplate.Application.Features.Templates.Commands.ChangeTemplatePricing;
using Qtemplate.Application.Features.Templates.Commands.ChangeTemplateStatus;
using Qtemplate.Application.Features.Templates.Commands.CreateTemplate;
using Qtemplate.Application.Features.Templates.Commands.DeleteTemplate;
using Qtemplate.Application.Features.Templates.Commands.DeleteTemplateImage;
using Qtemplate.Application.Features.Templates.Commands.DeleteTemplateVersion;
using Qtemplate.Application.Features.Templates.Commands.PublishTemplate;
using Qtemplate.Application.Features.Templates.Commands.SetPreviewUrl;
using Qtemplate.Application.Features.Templates.Commands.SetTemplateSale;
using Qtemplate.Application.Features.Templates.Commands.UpdatePreview;
using Qtemplate.Application.Features.Templates.Commands.UpdateTemplate;
using Qtemplate.Application.Features.Templates.Commands.UpdateThumbnail;
using Qtemplate.Application.Features.Templates.Queries.AdminGetTemplates;
using Qtemplate.Application.Features.Templates.Queries.GetTemplateDetail;
using Qtemplate.Application.Features.Templates.Queries.GetTemplateVersions;
using Qtemplate.Application.Services.Interfaces;
using System.Security.Claims;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/templates")]
[Authorize(Roles = "Admin")]
public class AdminTemplateController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileUploadService _fileUploadService;
    public AdminTemplateController(IMediator mediator, IFileUploadService fileUploadService)
    {
        _mediator = mediator;
        _fileUploadService = fileUploadService;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    private string? GetUserEmail() => User.FindFirstValue(ClaimTypes.Email);

    private string GetIpAddress() =>
        Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"].ToString()
            : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AdminGetTemplatesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTemplateDto dto)
    {
        var result = await _mediator.Send(new CreateTemplateCommand
        {
            Dto = dto,
            AdminId = GetUserId().ToString(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTemplateDto dto)
    {
        var result = await _mediator.Send(new UpdateTemplateCommand
        {
            Id = id,
            Dto = dto,
            AdminId = GetUserId().ToString(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var result = await _mediator.Send(new PublishTemplateCommand
        {
            Id = id,
            AdminId = GetUserId().ToString(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteTemplateCommand
        {
            Id = id,
            AdminId = GetUserId().ToString(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpPost("{id:guid}/thumbnail")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadThumbnail(Guid id, IFormFile file)
    {
        string url;
        try
        {
            await using var stream = file.OpenReadStream();
            url = await _fileUploadService.SaveThumbnailAsync(stream, file.FileName, file.Length);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }

        // Handler tự xóa thumbnail cũ bên trong
        var result = await _mediator.Send(new UpdateThumbnailCommand
        {
            TemplateId = id,
            ThumbnailUrl = url,
            AdminId = GetUserId().ToString(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });

        if (!result.Success) return NotFound(result);

        return Ok(ApiResponse<string>.Ok(url, "Upload thumbnail thành công"));
    }
    [HttpDelete("{id:guid}/thumbnail")]
    public async Task<IActionResult> DeleteThumbnail(Guid id)
    {
        // Handler tự fetch template, xóa file cũ, set null
        var result = await _mediator.Send(new UpdateThumbnailCommand
        {
            TemplateId = id,
            ThumbnailUrl = null!,
            AdminId = GetUserId().ToString(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });

        if (!result.Success) return NotFound(result);

        return Ok(ApiResponse<object>.Ok(null!, "Đã xóa thumbnail"));
    }

    [HttpPost("{id:guid}/preview")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> UploadPreview(Guid id, IFormFile file)
    {
        // Xóa preview cũ — KHÔNG xóa download file
        _fileUploadService.DeletePreview(id);

        string previewFolder;
        try
        {
            await using var stream = file.OpenReadStream();
         
            previewFolder = await _fileUploadService.SavePreviewZipAsync(
                stream, file.FileName, file.Length, id);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }

        await _mediator.Send(new UpdatePreviewCommand
        {
            TemplateId = id,
            PreviewFolder = previewFolder,
            PreviewType = "Iframe",
            AdminId = GetUserId().ToString(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });

        return Ok(ApiResponse<object>.Ok(new { previewFolder }, "Upload preview thành công"));
    }

    [HttpDelete("{id:guid}/preview")]
    public async Task<IActionResult> DeletePreview(Guid id)
    {
     
        _fileUploadService.DeletePreview(id);

        await _mediator.Send(new UpdatePreviewCommand
        {
            TemplateId = id,
            PreviewFolder = null,
            PreviewType = "None",
            AdminId = GetUserId().ToString(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });

        return Ok(ApiResponse<object>.Ok(null!, "Đã xóa preview"));
    }
    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> AddImage(Guid id, IFormFile file,
    [FromQuery] string? altText, [FromQuery] string type = "Screenshot", [FromQuery] int sortOrder = 0)
    {
        var result = await _mediator.Send(new AddTemplateImageCommand
        {
            TemplateId = id,
            File = file,
            AltText = altText,
            Type = type,
            SortOrder = sortOrder,
            AdminId = GetUserId().ToString(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("images/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int imageId)
    {
        var result = await _mediator.Send(new DeleteTemplateImageCommand
        {
            ImageId = imageId,
            AdminId = GetUserId().ToString(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpPatch("{id:guid}/preview-url")]
    public async Task<IActionResult> SetPreviewUrl(Guid id, [FromBody] SetPreviewUrlDto dto)
    {
        var result = await _mediator.Send(new SetPreviewUrlCommand
        {
            TemplateId = id,
            PreviewUrl = dto.PreviewUrl,
            AdminId = GetUserId().ToString(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetDetail(string slug)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new GetTemplateDetailQuery
        {
            Slug = slug,
            CurrentUserId = userId
        });
        return result.Success ? Ok(result) : NotFound(result);
    }
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto)
    {
        var result = await _mediator.Send(new ChangeTemplateStatusCommand
        {
            Id = id,
            Status = dto.Status,
            AdminId = GetUserId().ToString(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpPatch("{id:guid}/sale")]
    public async Task<IActionResult> SetSale(Guid id, [FromBody] SetSaleDto dto)
    {
        var result = await _mediator.Send(new SetTemplateSaleCommand
        {
            TemplateId = id,
            SalePrice = dto.SalePrice,
            SaleStartAt = dto.SaleStartAt,
            SaleEndAt = dto.SaleEndAt,
            AdminId = GetUserId().ToString(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpPatch("{id:guid}/pricing")]
    public async Task<IActionResult> ChangePricing(Guid id, [FromBody] ChangePricingDto dto)
    {
        var result = await _mediator.Send(new ChangeTemplatePricingCommand
        {
            TemplateId = id,
            IsFree = dto.IsFree,
            Price = dto.Price,
            AdminId = GetUserId().ToString(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpPost("{id:guid}/versions")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> AddVersion(Guid id, IFormFile file,
    [FromQuery] string version, [FromQuery] string? changeLog)
    {
        var result = await _mediator.Send(new AddTemplateVersionCommand
        {
            TemplateId = id,
            File = file,
            Version = version,
            ChangeLog = changeLog,
            AdminId = GetUserId().ToString(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpPost("{id:guid}/versions/link")]
    public async Task<IActionResult> AddVersionLink(Guid id, [FromBody] AddVersionLinkDto dto)
    {
        var result = await _mediator.Send(new AddTemplateVersionCommand
        {
            TemplateId = id,
            Version = dto.Version,
            ChangeLog = dto.ChangeLog,
            ExternalUrl = dto.ExternalUrl,
            StorageType = dto.StorageType,
            AdminId = GetUserId().ToString(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpGet("{id:guid}/versions")]
    public async Task<IActionResult> GetVersions(Guid id)
    {
        var result = await _mediator.Send(new GetTemplateVersionsQuery { TemplateId = id });
        return Ok(result);
    }
    [HttpDelete("{id:guid}/versions/{version}")]
    public async Task<IActionResult> DeleteVersion(Guid id, string version)
    {
        var result = await _mediator.Send(new DeleteTemplateVersionCommand
        {
            TemplateId = id,        // ← thêm
            Version = version,   // ← truyền "1.0.1"
            AdminId = GetUserId().ToString(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}