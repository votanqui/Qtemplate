using Qtemplate.Domain.Messages;

namespace Qtemplate.Application.Services.Interfaces;

public interface IEmailSender
{
    Task SendAsync(SendEmailMessage message);
}