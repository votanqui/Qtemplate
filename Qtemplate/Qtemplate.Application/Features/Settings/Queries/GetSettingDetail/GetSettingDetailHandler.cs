using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Setting;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Settings.Queries.GetSettingDetail;

public class GetSettingDetailHandler : IRequestHandler<GetSettingDetailQuery, ApiResponse<List<SettingGroupDto>>>
{
    private readonly ISettingRepository _settingRepo;
    public GetSettingDetailHandler(ISettingRepository settingRepo) => _settingRepo = settingRepo;

    public async Task<ApiResponse<List<SettingGroupDto>>> Handle(
        GetSettingDetailQuery request, CancellationToken cancellationToken)
    {
        var settings = await _settingRepo.GetDetailAsync(request.Group);

        var grouped = settings
            .GroupBy(s => s.Group)
            .Select(g => new SettingGroupDto
            {
                Group = g.Key,
                Settings = g.Select(s => new SettingItemDto
                {
                    Id = s.Id,
                    Key = s.Key,
                    Value = s.Value,
                    Group = s.Group,
                    Description = s.Description,
                    UpdatedAt = s.UpdatedAt
                }).ToList()
            })
            .ToList();

        return ApiResponse<List<SettingGroupDto>>.Ok(grouped);
    }
}