// Controllers/PreviewController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.Features.Preview.Queries;

namespace Qtemplate.Controllers;

[ApiController]
[Route("api/preview")]
public class PreviewController : ControllerBase
{
    private readonly IMediator _mediator;

    public PreviewController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET /api/preview/{templateId}        → serve index.html
    [HttpGet("{templateId:guid}")]
    public Task<IActionResult> ServeIndex(Guid templateId)
        => ServeFile(templateId, "index.html");

    // GET /api/preview/{templateId}/{**filePath} → serve bất kỳ file nào
    [HttpGet("{templateId:guid}/{**filePath}")]
    public async Task<IActionResult> ServeFile(Guid templateId, string filePath)
    {
        var result = await _mediator.Send(new ServePreviewFileQuery
        {
            TemplateId = templateId,
            FilePath = filePath,
        });

        if (!result.IsSuccess)
            return result.StatusCode == 404
                ? NotFound(new { message = result.Error })
                : BadRequest(new { message = result.Error });

        Response.Headers["Content-Disposition"] = "inline";
        Response.Headers["Cache-Control"] = "no-store";

        return File(result.FileBytes!, result.MimeType!);
    }
}