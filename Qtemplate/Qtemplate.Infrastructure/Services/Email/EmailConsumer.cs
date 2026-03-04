using MassTransit;
using Microsoft.Extensions.Logging;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Domain.Messages;

namespace Qtemplate.Infrastructure.Services.Email;

public class EmailConsumer : IConsumer<SendEmailMessage>
{
    private readonly IEmailService _emailService;
    private readonly IEmailLogRepository _emailLogRepo;
    private readonly ILogger<EmailConsumer> _logger;

    public EmailConsumer(
        IEmailService emailService,
        IEmailLogRepository emailLogRepo,
        ILogger<EmailConsumer> logger)
    {
        _emailService = emailService;
        _emailLogRepo = emailLogRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendEmailMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[EmailConsumer] Received — To={To} Template={Template} LogId={LogId}",
            msg.To, msg.Template, msg.EmailLogId);

        Domain.Entities.EmailLog? log = null;

        if (msg.EmailLogId.HasValue)
            log = await _emailLogRepo.GetByIdAsync(msg.EmailLogId.Value);

        try
        {
            await _emailService.SendDirectAsync(
                msg.To, msg.Subject, msg.Body, msg.Cc);

            if (log is not null)
            {
                log.Status = "Sent";
                log.SentAt = DateTime.UtcNow;
                await _emailLogRepo.UpdateAsync(log);
            }

            _logger.LogInformation(
                "[EmailConsumer] Sent successfully — To={To} LogId={LogId}",
                msg.To, msg.EmailLogId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[EmailConsumer] Failed — To={To} LogId={LogId} Error={Error}",
                msg.To, msg.EmailLogId, ex.Message);

            if (log is not null)
            {
                log.Status = "Failed";
                log.ErrorMessage = ex.Message;
                log.RetryCount++;
                await _emailLogRepo.UpdateAsync(log);
            }

            throw;
        }
    }
}