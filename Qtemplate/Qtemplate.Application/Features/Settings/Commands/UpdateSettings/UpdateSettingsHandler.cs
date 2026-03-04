using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Setting;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Settings.Commands.UpdateSettings;

public class UpdateSettingsHandler : IRequestHandler<UpdateSettingsCommand, ApiResponse<object>>
{
    private readonly ISettingRepository _settingRepo;
    public UpdateSettingsHandler(ISettingRepository settingRepo) => _settingRepo = settingRepo;

    public async Task<ApiResponse<object>> Handle(
        UpdateSettingsCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.Items)
            await _settingRepo.SetValueAsync(item.Key, item.Value, item.Group ?? "General");

        return ApiResponse<object>.Ok(null!, $"Đã cập nhật {request.Items.Count} setting");
    }
}