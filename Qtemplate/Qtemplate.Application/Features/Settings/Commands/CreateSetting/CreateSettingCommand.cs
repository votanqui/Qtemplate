using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Setting;

namespace Qtemplate.Application.Features.Settings.Commands.CreateSetting;

public class CreateSettingCommand : IRequest<ApiResponse<SettingItemDto>>
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string Group { get; set; } = "General";
    public string? Description { get; set; }
}