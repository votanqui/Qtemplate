namespace Qtemplate.Application.Services.Interfaces;

public record ModerationResult(bool IsApproved, string Reason);
public record PriorityResult(string Priority, string Reason);

public interface IAiModerationService
{
    // Sync — dùng trong Handler (chỉ rule-based, không chờ AI)
    ModerationResult ModerateReviewSync(string? title, string? comment);
    PriorityResult ClassifyTicketPrioritySync(string subject, string message);

    // Async — dùng trong Background Job (rule-based + OpenAI)
    Task<ModerationResult> ModerateReviewAsync(string? title, string? comment, int rating);
    Task<PriorityResult> ClassifyTicketPriorityAsync(string subject, string message);
}