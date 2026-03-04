using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Infrastructure.Services;

public class AiModerationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AiModerationBackgroundService> _logger;

    // Chạy mỗi 30 giây
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    public AiModerationBackgroundService(
        IServiceProvider services,
        ILogger<AiModerationBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AI Moderation Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingReviewsAsync(stoppingToken);
                await ProcessPendingTicketsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI Moderation background error");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    // ── Xử lý Review Pending ──────────────────────────────────────────────────
    private async Task ProcessPendingReviewsAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var reviewRepo = scope.ServiceProvider.GetRequiredService<IReviewRepository>();
        var aiService = scope.ServiceProvider.GetRequiredService<IAiModerationService>();

        // Lấy tối đa 10 review Pending mỗi lần
        var pending = await reviewRepo.GetPendingAiAsync(limit: 10);
        if (!pending.Any()) return;



        foreach (var review in pending)
        {
            try
            {
                var result = await aiService.ModerateReviewAsync(
                    review.Title, review.Comment, review.Rating);

                review.AiStatus = result.IsApproved ? "Approved" : "Rejected";
                review.AiReason = result.Reason;
                review.IsApproved = result.IsApproved;

                await reviewRepo.UpdateAsync(review);

                if (review.IsApproved)
                    await reviewRepo.UpdateTemplateRatingAsync(review.TemplateId);

                _logger.LogInformation(
                    "Review {Id}: AI={Status}, Reason={Reason}",
                    review.Id, review.AiStatus, result.Reason);

                // Delay nhỏ tránh rate limit
                await Task.Delay(500, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing review {Id}", review.Id);
            }
        }
    }

    // ── Xử lý Ticket Priority Pending ────────────────────────────────────────
    private async Task ProcessPendingTicketsAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var ticketRepo = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
        var aiService = scope.ServiceProvider.GetRequiredService<IAiModerationService>();

        // Lấy tối đa 10 ticket AiPriority=Pending
        var pending = await ticketRepo.GetPendingAiAsync(limit: 10);
        if (!pending.Any()) return;



        foreach (var ticket in pending)
        {
            try
            {
                var result = await aiService.ClassifyTicketPriorityAsync(
                    ticket.Subject, ticket.Message);

                ticket.Priority = result.Priority;
                ticket.AiPriorityReason = result.Reason;
                ticket.AiProcessed = true;
                await ticketRepo.UpdateAsync(ticket);

                _logger.LogInformation(
                    "Ticket {Id}: Priority={Priority}, Reason={Reason}",
                    ticket.Id, result.Priority, result.Reason);

                await Task.Delay(500, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ticket {Id}", ticket.Id);
            }
        }
    }
}