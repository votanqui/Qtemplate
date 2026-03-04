using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Setting;

namespace Qtemplate.Application.Features.Settings.Queries.GetSettingDetail;

public class GetSettingDetailQuery : IRequest<ApiResponse<List<SettingGroupDto>>>
{
    public string? Group { get; set; }
}

public class SettingGroupDto
{
    public string Group { get; set; } = string.Empty;
    public List<SettingItemDto> Settings { get; set; } = new();
}