using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Settings.Queries.GetSettings;

public class GetSettingsHandler : IRequestHandler<GetSettingsQuery, ApiResponse<Dictionary<string, string>>>
{
    private readonly ISettingRepository _settingRepo;
    public GetSettingsHandler(ISettingRepository settingRepo) => _settingRepo = settingRepo;

    public async Task<ApiResponse<Dictionary<string, string>>> Handle(
        GetSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _settingRepo.GetGroupAsync(request.Group);
        return ApiResponse<Dictionary<string, string>>.Ok(settings);
    }
}