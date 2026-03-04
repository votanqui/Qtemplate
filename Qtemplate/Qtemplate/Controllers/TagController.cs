using MediatR;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Tag;
using Qtemplate.Application.Features.Tags.Queries.GetTags;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Controllers;

[ApiController]
[Route("api/tags")]
public class TagController : ControllerBase
{
    private readonly IMediator _mediator;
    public TagController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetTagsQuery());
        return Ok(result);
    }
}