using MediatR;
using Qtemplate.Application.DTOs.Email;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Admin.Logs.Queries.GetEmailLogs
{
    public class GetEmailLogsHandler
       : IRequestHandler<GetEmailLogsQuery, ApiResponse<PaginatedResult<EmailLogDto>>>
    {
        private readonly IEmailLogRepository _repo;
        public GetEmailLogsHandler(IEmailLogRepository repo) => _repo = repo;

        public async Task<ApiResponse<PaginatedResult<EmailLogDto>>> Handle(
            GetEmailLogsQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _repo.GetPagedAsync(
                request.Status, request.Template, request.Page, request.PageSize);

            return ApiResponse<PaginatedResult<EmailLogDto>>.Ok(new PaginatedResult<EmailLogDto>
            {
                Items = items.Select(e => new EmailLogDto
                {
                    Id = e.Id,
                    To = e.To,
                    Cc = e.Cc,
                    Subject = e.Subject,
                    Template = e.Template,
                    Status = e.Status,
                    ErrorMessage = e.ErrorMessage,
                    RetryCount = e.RetryCount,
                    SentAt = e.SentAt,
                    CreatedAt = e.CreatedAt
                }).ToList(),
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }
}
