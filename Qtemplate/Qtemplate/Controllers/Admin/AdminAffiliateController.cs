using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Affiliate;
using Qtemplate.Application.Features.Affiliates.Commands;
using Qtemplate.Application.Features.Affiliates.Commands.ApproveAffiliate;
using Qtemplate.Application.Features.Affiliates.Queries;
using Qtemplate.Application.Features.Affiliates.Queries.GetAdminAffiliates;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/affiliates")]
[Authorize(Roles = "Admin")]
public class AdminAffiliateController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminAffiliateController(IMediator mediator) => _mediator = mediator;

    // GET /api/admin/affiliates?isActive=true&page=1
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAdminAffiliatesQuery
        {
            IsActive = isActive,
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }

    // PATCH /api/admin/affiliates/{id}/approve
    [HttpPatch("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveAffiliateDto dto)
    {
        var result = await _mediator.Send(new ApproveAffiliateCommand
        {
            AffiliateId = id,
            IsActive = dto.IsActive,
            CommissionRate = dto.CommissionRate
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}