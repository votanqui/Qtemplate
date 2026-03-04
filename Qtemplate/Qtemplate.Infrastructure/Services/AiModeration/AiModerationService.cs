using System.Text;
using System.Text.Json;
using Qtemplate.Application.Constants;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Infrastructure.Services;

public class AiModerationService : IAiModerationService
{
    private readonly HttpClient _http;
    private readonly ISettingRepository _settingRepo;

    private static readonly string[] BannedWords =
    [
        // Tục tĩu tiếng Việt (có dấu)
        "địt", "đụ", "lồn", "cặc", "buồi", "đít",
        "con mẹ", "mẹ mày", "má mày", "bố mày", "cha mày",
        "đĩ", "cave", "điếm",
        // Xúc phạm tiếng Việt (có dấu)
        "chó ngu", "chó chết", "súc vật", "thằng chó", "con chó",
        "óc lợn", "ngu như lợn", "thằng ngu", "con ngu",
        "đồ chó", "đồ điên", "thằng điên", "đần độn",
        // Không dấu / viết tắt cố tình tránh filter
        "dit", "du ma", "lon", "cac", "buoi",
        "dm", "dcm", "đcm", "vkl", "vcl", "clm", "đkm", "dkm",
        "con me", "me may", "ma may", "bo may", "cha may",
        "cho ngu", "cho chet", "suc vat", "thang cho", "con cho",
        "oc lon", "thang ngu", "con ngu", "do cho",
        "lua dao", "lua đảo",
        // Tiếng Anh
        "fuck", "shit", "bitch", "dick", "pussy", "cunt", "asshole",
        "motherfucker", "bastard", "retard"
    ];

    private static readonly string[] UrgentKeywords =
    [
        "không download được", "mất tiền", "thanh toán lỗi",
        "bị trừ tiền", "không nhận được", "lừa đảo", "hoàn tiền",
        "refund", "payment failed", "charged", "scam", "bị hack"
    ];

    private static readonly string[] HighKeywords =
    [
        "không truy cập", "lỗi nghiêm trọng", "crash",
        "không hoạt động", "bị lỗi", "không vào được",
        "bug", "broken", "not working", "lỗi hiển thị"
    ];

    private static readonly string[] LowKeywords =
    [
        "góp ý", "đề xuất", "hỏi thêm", "tư vấn",
        "suggestion", "feature request", "how to", "hướng dẫn",
        "cho hỏi", "muốn biết", "thắc mắc"
    ];

    public AiModerationService(HttpClient http, ISettingRepository settingRepo)
    {
        _http = http;
        _settingRepo = settingRepo;
    }

    // ════════════════════════════════════════════════════════════════
    // REVIEW — chỉ rule-based, AI xử lý ở background job
    // ════════════════════════════════════════════════════════════════
    public ModerationResult ModerateReviewSync(string? title, string? comment)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(comment))
            return new ModerationResult(true, "No text content");

        return CheckBannedWords(title, comment);
    }

    // Gọi bởi background job
    public async Task<ModerationResult> ModerateReviewAsync(
        string? title, string? comment, int rating)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(comment))
            return new ModerationResult(true, "No text content");

        // Rule-based trước
        var ruleResult = CheckBannedWords(title, comment);
        if (!ruleResult.IsApproved)
            return ruleResult;

        var apiKey = await _settingRepo.GetValueAsync(SettingKeys.OpenAiApiKey);
        var model = await _settingRepo.GetValueAsync(SettingKeys.OpenAiModel)
                     ?? "gpt-4o-mini";

        if (string.IsNullOrEmpty(apiKey))
            return new ModerationResult(true, "Rule-based approved");

        return await CallOpenAiForReviewAsync(apiKey, model, title, comment, rating);
    }

    // ════════════════════════════════════════════════════════════════
    // TICKET PRIORITY — rule-based sync + AI async
    // ════════════════════════════════════════════════════════════════
    public PriorityResult ClassifyTicketPrioritySync(string subject, string message)
    {
        return ClassifyByKeywords(subject, message)
               ?? new PriorityResult("Normal", "Rule-based default");
    }

    // Gọi bởi background job
    public async Task<PriorityResult> ClassifyTicketPriorityAsync(
        string subject, string message)
    {
        var ruleResult = ClassifyByKeywords(subject, message);
        if (ruleResult is not null)
            return ruleResult;

        var apiKey = await _settingRepo.GetValueAsync(SettingKeys.OpenAiApiKey);
        var model = await _settingRepo.GetValueAsync(SettingKeys.OpenAiModel)
                     ?? "gpt-4o-mini";

        if (string.IsNullOrEmpty(apiKey))
            return new PriorityResult("Normal", "Rule-based default");

        return await CallOpenAiForPriorityAsync(apiKey, model, subject, message);
    }

    // ════════════════════════════════════════════════════════════════
    // OpenAI calls
    // ════════════════════════════════════════════════════════════════
    private async Task<ModerationResult> CallOpenAiForReviewAsync(
        string apiKey, string model,
        string? title, string? comment, int rating)
    {
        var prompt = $$"""
            Kiểm duyệt review cho website bán template.

            Rating: {{rating}}/5
            Title: {{title ?? "(không có)"}}
            Comment: {{comment ?? "(không có)"}}

            Từ chối: tục tĩu, spam, không liên quan sản phẩm, thông tin nhạy cảm.
            Chấp nhận: review thật về template dù tiêu cực.

            Chỉ trả JSON: {"approved": true/false, "reason": "lý do ngắn"}
            """;

        var (success, text) = await CallOpenAiAsync(apiKey, model, prompt);
        if (!success)
            return new ModerationResult(false, text);

        try
        {
            using var result = JsonDocument.Parse(CleanJson(text));
            return new ModerationResult(
                result.RootElement.GetProperty("approved").GetBoolean(),
                result.RootElement.GetProperty("reason").GetString() ?? "");
        }
        catch
        {
            return new ModerationResult(false, "AI parse error");
        }
    }

    private async Task<PriorityResult> CallOpenAiForPriorityAsync(
        string apiKey, string model, string subject, string message)
    {
        var prompt = $$"""
            Phân loại mức độ ưu tiên support ticket website bán template.

            Subject: {{subject}}
            Message: {{message}}

            - Urgent: thanh toán, mất tiền, không download được sau khi mua
            - High: lỗi kỹ thuật nghiêm trọng
            - Normal: hỗ trợ thông thường
            - Low: góp ý, hỏi thông tin

            Chỉ trả JSON: {"priority": "Urgent/High/Normal/Low", "reason": "lý do ngắn"}
            """;

        var (success, text) = await CallOpenAiAsync(apiKey, model, prompt);
        if (!success)
            return new PriorityResult("Normal", $"AI error: {text}");

        try
        {
            using var result = JsonDocument.Parse(CleanJson(text));
            var priority = result.RootElement.GetProperty("priority").GetString() ?? "Normal";
            var reason = result.RootElement.GetProperty("reason").GetString() ?? "";
            var valid = new[] { "Low", "Normal", "High", "Urgent" };
            return new PriorityResult(valid.Contains(priority) ? priority : "Normal", reason);
        }
        catch
        {
            return new PriorityResult("Normal", "AI parse error");
        }
    }

    private async Task<(bool Success, string Text)> CallOpenAiAsync(
        string apiKey, string model, string prompt)
    {
        var body = new
        {
            model,
            max_tokens = 150,
            temperature = 0,
            messages = new[]
            {
                new { role = "system", content = "Chỉ trả về JSON, không giải thích." },
                new { role = "user",   content = prompt }
            }
        };

        try
        {
            var req = new HttpRequestMessage(HttpMethod.Post,
                "https://api.openai.com/v1/chat/completions");
            req.Headers.Add("Authorization", $"Bearer {apiKey}");
            req.Content = new StringContent(
                JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var res = await _http.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                return (false, $"OpenAI HTTP {(int)res.StatusCode}: {json[..Math.Min(150, json.Length)]}");

            using var doc = JsonDocument.Parse(json);
            var text = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";

            return (true, text);
        }
        catch (Exception ex)
        {
            return (false, $"Exception: {ex.Message[..Math.Min(100, ex.Message.Length)]}");
        }
    }

    // ════════════════════════════════════════════════════════════════
    // Helpers
    // ════════════════════════════════════════════════════════════════
    private static ModerationResult CheckBannedWords(string? title, string? comment)
    {
        var combined = $"{title} {comment}".ToLowerInvariant();
        foreach (var word in BannedWords)
            if (combined.Contains(word.ToLowerInvariant()))
                return new ModerationResult(false, "Nội dung chứa từ ngữ không phù hợp");
        return new ModerationResult(true, "Rule-based passed");
    }

    private static PriorityResult? ClassifyByKeywords(string subject, string message)
    {
        var combined = $"{subject} {message}".ToLowerInvariant();
        if (UrgentKeywords.Any(k => combined.Contains(k)))
            return new PriorityResult("Urgent", "Liên quan thanh toán hoặc tài chính");
        if (HighKeywords.Any(k => combined.Contains(k)))
            return new PriorityResult("High", "Lỗi kỹ thuật nghiêm trọng");
        if (LowKeywords.Any(k => combined.Contains(k)))
            return new PriorityResult("Low", "Góp ý hoặc câu hỏi thông tin");
        return null;
    }

    private static string CleanJson(string text)
    {
        var clean = text.Trim();
        if (clean.StartsWith("```"))
            clean = clean.TrimStart('`')
                .Replace("json\n", "").Replace("json\r\n", "")
                .TrimEnd('`').Trim();
        return clean;
    }
}