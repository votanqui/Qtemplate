using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Category;
using Qtemplate.Application.Features.Categories.Commands.CreateCategory;
using Qtemplate.Application.Features.Categories.Commands.DeleteCategory;
using Qtemplate.Application.Features.Categories.Commands.UpdateCategory;
using Qtemplate.Application.Features.Categories.Queries.GetCategories;
using System.Security.Claims;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Admin")]
public class AdminCategoryController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminCategoryController(IMediator mediator) => _mediator = mediator;

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private string? GetUserEmail() => User.FindFirstValue(ClaimTypes.Email);
    private string GetIpAddress() =>
        Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"].ToString()
            : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetCategoriesQuery { OnlyActive = false });
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var result = await _mediator.Send(new CreateCategoryCommand
        {
            Dto = dto,
            AdminId = GetUserId(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDto dto)
    {
        var result = await _mediator.Send(new UpdateCategoryCommand
        {
            Id = id,
            Dto = dto,
            AdminId = GetUserId(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand
        {
            Id = id,
            AdminId = GetUserId(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}