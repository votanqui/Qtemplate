using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Media;
using Qtemplate.Application.Features.Media.Commands.UploadMedia;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Media.Queries.GetMediaList;

public class GetMediaListHandler
    : IRequestHandler<GetMediaListQuery, ApiResponse<PaginatedResult<MediaFileDto>>>
{
    private readonly IMediaFileRepository _mediaRepo;
    public GetMediaListHandler(IMediaFileRepository mediaRepo) => _mediaRepo = mediaRepo;

    public async Task<ApiResponse<PaginatedResult<MediaFileDto>>> Handle(
        GetMediaListQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _mediaRepo.GetListAsync(
            request.TemplateId, request.Page, request.PageSize);

        return ApiResponse<PaginatedResult<MediaFileDto>>.Ok(new PaginatedResult<MediaFileDto>
        {
            Items = items.Select(UploadMediaHandler.ToDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}