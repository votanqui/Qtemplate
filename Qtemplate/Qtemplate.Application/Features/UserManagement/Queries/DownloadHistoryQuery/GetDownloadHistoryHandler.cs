using MediatR;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.UserManagement.Queries.DownloadHistoryQuery
{
    public class GetDownloadHistoryHandler
      : IRequestHandler<GetDownloadHistoryQuery, ApiResponse<PagedResultDto<DownloadHistoryItemDto>>>
    {
        private readonly IUserDownloadRepository _downloadRepo;

        public GetDownloadHistoryHandler(IUserDownloadRepository downloadRepo)
            => _downloadRepo = downloadRepo;

        public async Task<ApiResponse<PagedResultDto<DownloadHistoryItemDto>>> Handle(
            GetDownloadHistoryQuery request, CancellationToken cancellationToken)
        {
            var (downloads, total) = await _downloadRepo.GetPagedByUserIdAsync(request.UserId, request.Page, request.PageSize);

            var items = downloads.Select(d => new DownloadHistoryItemDto
            {
                TemplateId = d.TemplateId,
                TemplateName = d.Template?.Name ?? string.Empty,
                ThumbnailUrl = d.Template?.ThumbnailUrl,
                DownloadCount = d.DownloadCount,
                LastDownloadAt = d.LastDownloadAt
            }).ToList();

            return ApiResponse<PagedResultDto<DownloadHistoryItemDto>>.Ok(new PagedResultDto<DownloadHistoryItemDto>
            {
                Items = items,
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }
}
