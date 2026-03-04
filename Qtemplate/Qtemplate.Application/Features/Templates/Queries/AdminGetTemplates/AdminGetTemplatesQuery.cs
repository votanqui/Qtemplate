using MediatR;
using Qtemplate.Application.DTOs.Template.Admin;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Templates.Queries.AdminGetTemplates
{
    public class AdminGetTemplatesQuery : IRequest<ApiResponse<PaginatedResult<AdminTemplateListDto>>>
    {
        public string? Search { get; set; }
        public string? Status { get; set; }   // Draft / Published / Hidden
        public int? CategoryId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
