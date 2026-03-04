using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Tag;

namespace Qtemplate.Application.Features.Tags.Queries.GetTags;

public class GetTagsQuery : IRequest<ApiResponse<List<TagDto>>> { }