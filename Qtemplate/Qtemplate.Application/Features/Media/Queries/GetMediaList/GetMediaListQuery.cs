using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Media;

namespace Qtemplate.Application.Features.Media.Queries.GetMediaList;

public class GetMediaListQuery : IRequest<ApiResponse<PaginatedResult<MediaFileDto>>>
{
    public Guid? TemplateId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}