using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Setting;

namespace Qtemplate.Application.Features.Settings.Commands.UpdateSettings;

public class UpdateSettingsCommand : IRequest<ApiResponse<object>>
{
    public List<UpdateSettingDto> Items { get; set; } = new();
}