using MediatR;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Banners.Commands.DeleteBanner
{
    public class DeleteBannerCommand : IRequest<ApiResponse<object>>
    {
        public int Id { get; set; }
    }
}
