using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Infrastructure.Services.Email;

public class EmailRetryBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailRetryBackgroundService> _logger;

    // Chạy retry mỗi 2 phút
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(2);

    public EmailRetryBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<EmailRetryBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[EmailRetry] Background service started");

        // Chạy ngay lúc start để xử lý pending từ lần down trước
        await ProcessPendingEmailsAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_interval, stoppingToken);
            await ProcessPendingEmailsAsync();
        }
    }

    private async Task ProcessPendingEmailsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var emailLogRepo = scope.ServiceProvider.GetRequiredService<IEmailLogRepository>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        try
        {
            // Lấy tối đa 20 email Pending, RetryCount < 3
            var pending = await emailLogRepo.GetPendingAsync(20);

            if (!pending.Any()) return;

            _logger.LogInformation(
                "[EmailRetry] Found {Count} pending emails, processing...", pending.Count);

            foreach (var log in pending)
            {
                try
                {
                    await emailService.SendDirectAsync(log.To, log.Subject, log.Body, log.Cc);

                    log.Status = "Sent";
                    log.SentAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "[EmailRetry] Sent — Id={Id} To={To} Template={Template}",
                        log.Id, log.To, log.Template);
                }
                catch (Exception ex)
                {
                    log.RetryCount++;
                    log.ErrorMessage = ex.Message;

                    // Hết 3 lần → đánh dấu Failed, không retry nữa
                    if (log.RetryCount >= 3)
                    {
                        log.Status = "Failed";
                        _logger.LogError(
                            "[EmailRetry] Permanently failed after 3 retries — Id={Id} To={To} Error={Error}",
                            log.Id, log.To, ex.Message);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "[EmailRetry] Retry {Count}/3 failed — Id={Id} To={To} Error={Error}",
                            log.RetryCount, log.Id, log.To, ex.Message);
                    }
                }
                finally
                {
                    await emailLogRepo.UpdateAsync(log);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EmailRetry] Error processing pending emails");
        }
    }
}