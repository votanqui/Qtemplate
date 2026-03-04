using MassTransit;
using Microsoft.Extensions.Logging;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Domain.Messages;

namespace Qtemplate.Infrastructure.Services.Email;

public class EmailSender : IEmailSender
{
    private readonly IPublishEndpoint _bus;
    private readonly IEmailLogRepository _emailLogRepo;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(
        IPublishEndpoint bus,
        IEmailLogRepository emailLogRepo,
        ILogger<EmailSender> logger)
    {
        _bus = bus;
        _emailLogRepo = emailLogRepo;
        _logger = logger;
    }

    public async Task SendAsync(SendEmailMessage message)
    {
        var log = new EmailLog
        {
            To = message.To,
            Cc = message.Cc,
            Subject = message.Subject,
            Body = message.Body,
            Template = message.Template,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _emailLogRepo.AddAsync(log);
        message.EmailLogId = log.Id;

        try
        {
            await _bus.Publish(message);
            _logger.LogInformation(
                "[EmailSender] Published to RabbitMQ — LogId={LogId}", log.Id);
        }
        catch (Exception ex)
        {
            // RabbitMQ down — log vẫn Pending, background service sẽ retry
            _logger.LogWarning(
                "[EmailSender] RabbitMQ unavailable, email queued for retry — LogId={LogId} Error={Error}",
                log.Id, ex.Message);
        }
    }
}