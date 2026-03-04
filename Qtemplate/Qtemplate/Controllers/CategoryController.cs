using MediatR;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Category;
using Qtemplate.Application.Features.Categories.Queries.GetCategories;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;
    public CategoryController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetCategoriesQuery { OnlyActive = true });
        return Ok(result);
    }
}