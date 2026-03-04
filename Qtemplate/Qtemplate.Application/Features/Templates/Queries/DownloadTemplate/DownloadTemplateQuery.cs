using MediatR;
using Qtemplate.Application.DTOs.payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Templates.Queries.DownloadTemplate
{
    public class DownloadTemplateQuery : IRequest<DownloadTemplateResult>
    {
        public string Slug { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
