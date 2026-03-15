namespace Qtemplate.Application.Constants;

public static class SettingKeys
{
    // SePay / Payment
    public const string SepayBankCode = "sepay_bank_code";
    public const string SepayAccountNumber = "sepay_account_number";
    public const string SepayApiKey = "sepay_api_key";
    public const string SepayQrBaseUrl = "sepay_qr_base_url";  // https://qr.sepay.vn/img

    // Site
    public const string SiteName = "site_name";
    public const string SiteUrl = "site_url";
    public const string OpenAiApiKey = "openai_api_key";
    public const string OpenAiModel = "openai_model";
    public const string GeminiApiKey = "gemini_api_key";
    public const string GeminiModel = "gemini_model";
    public const string AffiliateCommissionRate = "Affiliate.CommissionRate";
    public const string AffiliateAutoApproveDays = "affiliate.auto_approve_days";
    // ── Security Scanner ─────────────────────────────────────────────────────

    public const string SecurityScanIntervalMinutes = "security_scan_interval_minutes";
    public const string SecurityTimeWindowMinutes = "security_time_window_minutes";
    public const string SecurityMaxRequestsPerWindow = "security_max_requests_per_window";
    public const string SecurityMaxFailedLogins = "security_max_failed_logins";
    public const string SecurityMaxErrorRatePercent = "security_max_error_rate_percent";
    public const string SecurityMaxScanRequests = "security_max_scan_requests";
    public const string SecurityMaxReviewSpam = "security_max_review_spam";
    public const string SecurityMaxOrderCancels = "security_max_order_cancels";
    public const string SecurityBlockDurationHours = "security_block_duration_hours";
   
    public const string OrderReminderEnabled = "order.reminder_enabled";
    /// Số phút sau khi tạo đơn thì gửi nhắc nhở (default: 30)
    public const string OrderPaymentReminderMinutes = "order.payment_reminder_minutes";
    /// Số phút sau khi tạo đơn thì auto-cancel (default: 60)
    public const string OrderAutoCancelMinutes = "order.auto_cancel_minutes";

    public const string RefreshTokenRetentionDays = "auth.refresh_token_retention_days";
}