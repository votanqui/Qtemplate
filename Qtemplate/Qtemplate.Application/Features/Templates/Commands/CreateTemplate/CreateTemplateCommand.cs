using MediatR;
using Qtemplate.Application.DTOs.Template.Admin;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Templates.Commands.CreateTemplate
{
    public class CreateTemplateCommand : IRequest<ApiResponse<Guid>>
    {
        public CreateTemplateDto Dto { get; set; } = new();
        public string? AdminId { get; set; }
        public string? AdminEmail { get; set; }
        public string? IpAddress { get; set; }
    }
}
