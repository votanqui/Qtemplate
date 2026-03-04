using MediatR;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.UserManagement.Commands.ToggleWishlists
{
    public class ToggleWishlistCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public Guid TemplateId { get; set; }
    }
}
