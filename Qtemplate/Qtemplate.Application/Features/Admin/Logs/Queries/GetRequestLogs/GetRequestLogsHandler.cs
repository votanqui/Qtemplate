using MediatR;
using Qtemplate.Application.DTOs.Request;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Admin.Logs.Queries.GetRequestLogs
{
    public class GetRequestLogsHandler
      : IRequestHandler<GetRequestLogsQuery, ApiResponse<PaginatedResult<RequestLogDto>>>
    {
        private readonly IRequestLogRepository _repo;
        public GetRequestLogsHandler(IRequestLogRepository repo) => _repo = repo;

        public async Task<ApiResponse<PaginatedResult<RequestLogDto>>> Handle(
            GetRequestLogsQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _repo.GetPagedAsync(
                request.Ip, request.UserId, request.Endpoint,
                request.StatusCode, request.Page, request.PageSize);

            return ApiResponse<PaginatedResult<RequestLogDto>>.Ok(new PaginatedResult<RequestLogDto>
            {
                Items = items.Select(r => new RequestLogDto
                {
                    Id = r.Id,
                    IpAddress = r.IpAddress,
                    UserId = r.UserId,
                    Endpoint = r.Endpoint,
                    Method = r.Method,
                    StatusCode = r.StatusCode,
                    ResponseTimeMs = r.ResponseTimeMs,
                    UserAgent = r.UserAgent,
                    Referer = r.Referer,
                    CreatedAt = r.CreatedAt
                }).ToList(),
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }
}
