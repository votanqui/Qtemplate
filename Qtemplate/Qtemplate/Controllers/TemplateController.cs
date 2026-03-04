using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.Features.Templates.Queries.DownloadTemplate;
using Qtemplate.Application.Features.Templates.Queries.GetTemplateDetail;
using Qtemplate.Application.Features.Templates.Queries.GetTemplates;
using System.Security.Claims;

namespace Qtemplate.Controllers;

[ApiController]
[Route("api/templates")]
public class TemplateController : ControllerBase
{
    private readonly IMediator _mediator;
    public TemplateController(IMediator mediator) => _mediator = mediator;

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
    private string GetIpAddress() =>
    Request.Headers.ContainsKey("X-Forwarded-For")
        ? Request.Headers["X-Forwarded-For"].ToString()
        : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    // GET /api/templates?search=&categorySlug=&tagSlug=&isFree=&minPrice=&maxPrice=&sortBy=newest&page=1&pageSize=12
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetTemplatesQuery query)
    {
        query.CurrentUserId = GetUserId();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // GET /api/templates/{slug}
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetDetail(string slug)
    {
        var result = await _mediator.Send(new GetTemplateDetailQuery
        {
            Slug = slug,
            CurrentUserId = GetUserId()
        });
        return result.Success ? Ok(result) : NotFound(result);
    }
    [HttpGet("{slug}/download")]
    [Authorize]
    public async Task<IActionResult> Download(string slug)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new DownloadTemplateQuery
        {
            Slug = slug,
            UserId = userId,
            IpAddress = GetIpAddress(),
            UserAgent = Request.Headers.UserAgent.ToString()
        });

        if (!result.Success)
            return BadRequest(new { success = false, message = result.ErrorMessage });

        // External → trả URL về cho frontend tự mở
        if (result.IsExternal)
            return Ok(new
            {
                success = true,
                isExternal = true,
                redirectUrl = result.RedirectUrl
            });

        // Local → stream file
        var stream = new FileStream(result.FilePath!, FileMode.Open, FileAccess.Read);
        Response.Headers.Append("Content-Disposition",
            $"attachment; filename=\"{result.FileName}\"");
        return File(stream, result.ContentType, result.FileName);
    }
}