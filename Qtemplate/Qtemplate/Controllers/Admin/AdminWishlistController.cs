using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.Features.UserManagement.Queries.AdminGetWishlists;
using Qtemplate.Application.Features.UserManagement.Queries.GetTopWishlisted;

namespace Qtemplate.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/wishlists")]
    [Authorize(Roles = "Admin")]
    public class AdminWishlistController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AdminWishlistController(IMediator mediator) => _mediator = mediator;

        // GET /api/admin/wishlists
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] AdminGetWishlistsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // GET /api/admin/wishlists/top
        [HttpGet("top")]
        public async Task<IActionResult> GetTop([FromQuery] int top = 10)
        {
            var result = await _mediator.Send(new GetTopWishlistedQuery { Top = top });
            return Ok(result);
        }
    }
}
