using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Banner;
using Qtemplate.Application.Features.Banners.Commands;
using Qtemplate.Application.Features.Banners.Commands.CreateBanne;
using Qtemplate.Application.Features.Banners.Commands.DeleteBanner;
using Qtemplate.Application.Features.Banners.Commands.UpdateBanner;
using Qtemplate.Application.Features.Banners.Queries;
using Qtemplate.Application.Features.Banners.Queries.AdminGetBanner;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/banners")]
[Authorize(Roles = "Admin")]
public class AdminBannerController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminBannerController(IMediator mediator) => _mediator = mediator;

    // GET /api/admin/banners
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAdminBannersQuery
        {
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }

    // POST /api/admin/banners
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertBannerDto dto)
    {
        var result = await _mediator.Send(new CreateBannerCommand { Data = dto });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PUT /api/admin/banners/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertBannerDto dto)
    {
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
}