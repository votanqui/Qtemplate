using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Tag;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Tags.Queries.GetTags;

public class GetTagsHandler : IRequestHandler<GetTagsQuery, ApiResponse<List<TagDto>>>
{
    private readonly ITagRepository _tagRepo;
    public GetTagsHandler(ITagRepository tagRepo) => _tagRepo = tagRepo;

    public async Task<ApiResponse<List<TagDto>>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _tagRepo.GetAllAsync();
        var dtos = tags.Select(t => new TagDto { Id = t.Id, Name = t.Name, Slug = t.Slug }).ToList();
        return ApiResponse<List<TagDto>>.Ok(dtos);
    }
}