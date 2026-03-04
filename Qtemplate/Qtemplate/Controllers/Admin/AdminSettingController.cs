using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Setting;
using Qtemplate.Application.Features.Settings.Commands.CreateSetting;
using Qtemplate.Application.Features.Settings.Commands.UpdateOneSetting;
using Qtemplate.Application.Features.Settings.Commands.UpdateSettings;
using Qtemplate.Application.Features.Settings.Queries.GetSettingDetail;
using Qtemplate.Application.Features.Settings.Queries.GetSettings;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/settings")]
[Authorize(Roles = "Admin")]
public class AdminSettingController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminSettingController(IMediator mediator) => _mediator = mediator;

    // POST /api/admin/settings
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSettingDto dto)
    {
        var result = await _mediator.Send(new CreateSettingCommand
        {
            Key = dto.Key,
            Value = dto.Value,
            Group = dto.Group,
            Description = dto.Description
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
    // GET /api/admin/settings?group=Payment → Dictionary {key: value}
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? group)
    {
        var result = await _mediator.Send(new GetSettingsQuery { Group = group });
        return Ok(result);
    }

    // GET /api/admin/settings/detail?group=Payment → List grouped cho UI
    [HttpGet("detail")]
    public async Task<IActionResult> GetDetail([FromQuery] string? group)
    {
        var result = await _mediator.Send(new GetSettingDetailQuery { Group = group });
        return Ok(result);
    }

    // PUT /api/admin/settings → Bulk update
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] List<UpdateSettingDto> items)
    {
        var result = await _mediator.Send(new UpdateSettingsCommand { Items = items });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PATCH /api/admin/settings/{key} → Update 1 setting
    [HttpPatch("{key}")]
    public async Task<IActionResult> UpdateOne(string key, [FromBody] PatchSettingDto dto)
    {
        var result = await _mediator.Send(new UpdateOneSettingCommand
        {
            Key = key,
            Value = dto.Value
        });
        return result.Success ? Ok(result) : NotFound(result);
    }
}

public class PatchSettingDto
{
    public string Value { get; set; } = string.Empty;
}