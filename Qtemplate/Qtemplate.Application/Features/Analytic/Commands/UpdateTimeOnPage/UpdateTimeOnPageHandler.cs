using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Analytic.Commands.UpdateTimeOnPage;

public class UpdateTimeOnPageHandler : IRequestHandler<UpdateTimeOnPageCommand, ApiResponse<object>>
{
    private readonly IAnalyticsRepository _analyticsRepo;
    public UpdateTimeOnPageHandler(IAnalyticsRepository analyticsRepo) => _analyticsRepo = analyticsRepo;

    public async Task<ApiResponse<object>> Handle(
        UpdateTimeOnPageCommand request, CancellationToken cancellationToken)
    {
        await _analyticsRepo.UpdateTimeOnPageAsync(request.SessionId, request.PageUrl, request.Seconds);
        return ApiResponse<object>.Ok(null!, "Updated");
    }
}