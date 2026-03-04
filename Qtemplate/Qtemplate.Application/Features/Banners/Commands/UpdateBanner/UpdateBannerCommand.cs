using MediatR;
using Qtemplate.Application.DTOs.Banner;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Banners.Commands.UpdateBanner
{
    public class UpdateBannerCommand : IRequest<ApiResponse<BannerDto>>
    {
        public int Id { get; set; }
        public UpsertBannerDto Data { get; set; } = null!;
    }
}
