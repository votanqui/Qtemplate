using MediatR;
using Qtemplate.Application.DTOs.Template.Admin;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Templates.Commands.UpdateTemplate
{
    public class UpdateTemplateCommand : IRequest<ApiResponse<object>>
    {
        public Guid Id { get; set; }
        public UpdateTemplateDto Dto { get; set; } = new();
        public string? AdminId { get; set; }
        public string? AdminEmail { get; set; }
        public string? IpAddress { get; set; }
    }
}
