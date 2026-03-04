using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Setting;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Settings.Commands.CreateSetting;

public class CreateSettingHandler : IRequestHandler<CreateSettingCommand, ApiResponse<SettingItemDto>>
{
    private readonly ISettingRepository _settingRepo;
    public CreateSettingHandler(ISettingRepository settingRepo) => _settingRepo = settingRepo;

    public async Task<ApiResponse<SettingItemDto>> Handle(
        CreateSettingCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra key đã tồn tại chưa
        var existing = await _settingRepo.GetValueAsync(request.Key);
        if (existing is not null)
            return ApiResponse<SettingItemDto>.Fail($"Key '{request.Key}' đã tồn tại");

        await _settingRepo.SetValueAsync(
            request.Key,
            request.Value ?? "",
            request.Group,
            request.Description);

        // Lấy lại để trả về đầy đủ thông tin
        var settings = await _settingRepo.GetDetailAsync();
        var created = settings.FirstOrDefault(s => s.Key == request.Key);

        return ApiResponse<SettingItemDto>.Ok(new SettingItemDto
        {
            Id = created?.Id ?? 0,
            Key = request.Key,
            Value = request.Value,
            Group = request.Group,
            Description = request.Description,
            UpdatedAt = DateTime.UtcNow
        }, $"Đã tạo setting '{request.Key}'");
    }
}