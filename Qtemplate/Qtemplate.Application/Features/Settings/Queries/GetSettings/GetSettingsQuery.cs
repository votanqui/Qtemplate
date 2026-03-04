using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Setting;

namespace Qtemplate.Application.Features.Settings.Queries.GetSettings;

public class GetSettingsQuery : IRequest<ApiResponse<Dictionary<string, string>>>
{
    public string? Group { get; set; }
}