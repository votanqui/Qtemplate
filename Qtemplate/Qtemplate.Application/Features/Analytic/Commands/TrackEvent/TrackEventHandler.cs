using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Analytic.Commands.TrackEvent;

public class TrackEventHandler : IRequestHandler<TrackEventCommand, ApiResponse<object>>
{
    private readonly IAnalyticsRepository _analyticsRepo;
    public TrackEventHandler(IAnalyticsRepository analyticsRepo) => _analyticsRepo = analyticsRepo;

    public async Task<ApiResponse<object>> Handle(
        TrackEventCommand request, CancellationToken cancellationToken)
    {
        await _analyticsRepo.AddAsync(new Analytics
        {
            SessionId = request.SessionId,
            UserId = request.UserId,
            IpAddress = request.IpAddress,
            PageUrl = request.PageUrl,
            Referer = request.Referer,
            UTMSource = request.UTMSource,
            UTMMedium = request.UTMMedium,
            UTMCampaign = request.UTMCampaign,
            AffiliateCode = request.AffiliateCode,
            TimeOnPage = request.TimeOnPage,
            CreatedAt = DateTime.UtcNow
        });

        return ApiResponse<object>.Ok(null!, "Tracked");
    }
}