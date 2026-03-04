using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Settings.Commands.UpdateOneSetting;

public class UpdateOneSettingHandler : IRequestHandler<UpdateOneSettingCommand, ApiResponse<object>>
{
    private readonly ISettingRepository _settingRepo;
    public UpdateOneSettingHandler(ISettingRepository settingRepo) => _settingRepo = settingRepo;

    public async Task<ApiResponse<object>> Handle(
        UpdateOneSettingCommand request, CancellationToken cancellationToken)
    {
        var existing = await _settingRepo.GetDetailAsync();
        var setting = existing.FirstOrDefault(s => s.Key == request.Key);

        if (setting is null)
            return ApiResponse<object>.Fail($"Không tìm thấy setting: {request.Key}");

        await _settingRepo.SetValueAsync(
            request.Key,
            request.Value,
            request.Group ?? setting.Group);

        return ApiResponse<object>.Ok(null!, $"Đã cập nhật {request.Key}");
    }
}