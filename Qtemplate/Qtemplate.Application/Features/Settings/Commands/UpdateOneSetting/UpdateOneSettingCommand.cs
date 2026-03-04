using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Settings.Commands.UpdateOneSetting;

public class UpdateOneSettingCommand : IRequest<ApiResponse<object>>
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Group { get; set; }
}