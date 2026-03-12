// Qtemplate.Application/Services/EmailTemplates.cs
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;

namespace Qtemplate.Application.Services;

public static class EmailTemplates
{
    private static string BaseLayout(string title, string accentColor, string content)
    {
        return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            background: #f4f6f8;
            font-family: 'Segoe UI', Arial, sans-serif;
            color: #333;
        }}

        .wrapper {{
            max-width: 600px;
            margin: 40px auto;
            background: #fff;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 20px rgba(0,0,0,0.08);
        }}

        .header {{
            background: {accentColor};
            padding: 32px 40px;
            text-align: center;
        }}

        .header h1 {{
            margin: 0;
            color: #fff;
            font-size: 22px;
            font-weight: 600;
        }}

        .logo {{
            font-size: 28px;
            font-weight: 700;
            color: #fff;
            margin-bottom: 8px;
            display: block;
        }}

        .body {{
            padding: 36px 40px;
            line-height: 1.7;
            font-size: 15px;
        }}

        .btn {{
            display: inline-block;
            margin: 20px 0;
            padding: 14px 32px;
            background: {accentColor};
            color: #fff !important;
            text-decoration: none;
            border-radius: 8px;
            font-weight: 600;
        }}

        .info-box {{
            background: #f8f9fa;
            border-left: 4px solid {accentColor};
            padding: 14px 18px;
            border-radius: 4px;
            margin: 20px 0;
            font-size: 14px;
        }}

        .footer {{
            background: #f8f9fa;
            padding: 20px 40px;
            text-align: center;
            font-size: 13px;
            color: #888;
        }}
    </style>
</head>
<body>
    <div class='wrapper'>
        <div class='header'>
            <span class='logo'>Qtemplate</span>
            <h1>{title}</h1>
        </div>

        <div class='body'>
            {content}
        </div>

        <div class='footer'>
            &copy; {DateTime.UtcNow.Year} Qtemplate.
            Nếu bạn không thực hiện hành động này, hãy bỏ qua email này.
        </div>
    </div>
</body>
</html>";
    }

    // ── 1. Đăng ký — xác thực email ──────────────────────────────────────────
    public static string VerifyEmail(string fullName, string verifyUrl) =>
        BaseLayout("Xác minh tài khoản", "#4F46E5", $"""
            <p>Xin ch&#224;o <strong>{fullName}</strong>,</p>
            <p>C&#7843;m &#417;n b&#7841;n &#273;&#227; &#273;&#259;ng k&#253; t&#224;i kho&#7843;n t&#7841;i <strong>Qtemplate</strong>. Vui l&#242;ng nh&#7845;n n&uacute;t b&ecirc;n d&#432;&#7899;i &#273;&#7875; x&aacute;c minh &#273;&#7883;a ch&#7881; email v&#224; k&iacute;ch ho&#7841;t t&#224;i kho&#7843;n.</p>
            <div style="text-align:center">
                <a href="{verifyUrl}" class="btn">&#9993; Xác minh Email</a>
            </div>
            <div class="info-box">
                &#9201; Li&ecirc;n k&#7871;t n&agrave;y s&#7869; <strong>h&#7871;t h&#7841;n sau 24 gi&#7901;</strong>. N&#7871;u h&#7871;t h&#7841;n, b&#7841;n c&oacute; th&#7875; y&ecirc;u c&#7847;u g&#7917;i l&#7841;i.
            </div>
            <hr class="divider"/>
            <p style="font-size:13px;color:#888">Ho&#7863;c copy &#273;&#432;&#7901;ng d&#7851;n sau v&agrave;o tr&igrave;nh duy&#7879;t:<br/><a href="{verifyUrl}" style="color:#4F46E5;word-break:break-all">{verifyUrl}</a></p>
            """);

    // ── 2. Xác thực thành công — chào mừng ───────────────────────────────────
    public static string WelcomeAfterVerify(string fullName, string loginUrl) =>
        BaseLayout("Chào mừng bạn đến với Qtemplate!", "#10B981", $"""
            <p>Xin ch&#224;o <strong>{fullName}</strong>,</p>
            <p>&#127881; T&#224;i kho&#7843;n c&#7911;a b&#7841;n &#273;&#227; &#273;&#432;&#7907;c <strong>x&aacute;c th&#7921;c th&agrave;nh c&ocirc;ng</strong>! Ch&#224;o m&#7915;ng b&#7841;n &#273;&#7871;n v&#7899;i c&#7897;ng &#273;&#7891;ng <strong>Qtemplate</strong>.</p>
            <p>B&#7841;n c&oacute; th&#7875; b&#7855;t &#273;&#7847;u kh&aacute;m ph&aacute; h&#224;ng tr&#259;m template ch&#7845;t l&#432;&#7907;ng cao ngay b&acirc;y gi&#7901;.</p>
            <div style="text-align:center">
                <a href="{loginUrl}" class="btn">&#128640; Bắt đầu khám phá</a>
            </div>
            <hr class="divider"/>
            <div class="info-box">
                &#128161; <strong>M&#7865;o:</strong> H&#227;y ki&#7875;m tra m&#7909;c <em>Template m&#7899;i</em> v&#224; <em>N&#7893;i b&#7853;t</em> &#273;&#7875; kh&ocirc;ng b&#7887; l&#7905; nh&#7919;ng s&#7843;n ph&#7849;m hot nh&#7845;t.
            </div>
            """);

    // ── 3. Quên mật khẩu ─────────────────────────────────────────────────────
    public static string ForgotPassword(string fullName, string resetUrl) =>
        BaseLayout("Đặt lại mật khẩu", "#F59E0B", $"""
            <p>Xin ch&#224;o <strong>{fullName}</strong>,</p>
            <p>Ch&uacute;ng t&ocirc;i nh&#7853;n &#273;&#432;&#7907;c y&ecirc;u c&#7847;u &#273;&#7863;t l&#7841;i m&#7853;t kh&#7849;u cho t&#224;i kho&#7843;n g&#7855;n v&#7899;i email n&agrave;y. Nh&#7845;n n&uacute;t b&ecirc;n d&#432;&#7899;i &#273;&#7875; ti&#7871;n h&agrave;nh.</p>
            <div style="text-align:center">
                <a href="{resetUrl}" class="btn">&#128273; Đặt lại mật khẩu</a>
            </div>
            <div class="info-box">
                &#9201; Li&ecirc;n k&#7871;t n&agrave;y s&#7869; <strong>h&#7871;t h&#7841;n sau 1 gi&#7901;</strong>.<br/>
                &#128274; N&#7871;u b&#7841;n <strong>kh&ocirc;ng</strong> y&ecirc;u c&#7847;u &#273;&#7863;t l&#7841;i m&#7853;t kh&#7849;u, h&#227;y b&#7887; qua email n&agrave;y &mdash; t&#224;i kho&#7843;n v&#7851;n an to&agrave;n.
            </div>
            <hr class="divider"/>
            <p style="font-size:13px;color:#888">Ho&#7863;c copy &#273;&#432;&#7901;ng d&#7851;n sau v&agrave;o tr&igrave;nh duy&#7879;t:<br/><a href="{resetUrl}" style="color:#F59E0B;word-break:break-all">{resetUrl}</a></p>
            """);

    // ── 4. Thay đổi mật khẩu thành công ─────────────────────────────────────
    public static string PasswordChanged(string fullName, string ipAddress, string supportUrl) =>
        BaseLayout("Mật khẩu đã được thay đổi", "#EF4444", $"""
            <p>Xin ch&#224;o <strong>{fullName}</strong>,</p>
            <p>M&#7853;t kh&#7849;u t&#224;i kho&#7843;n c&#7911;a b&#7841;n v&#7915;a &#273;&#432;&#7907;c <strong>thay &#273;&#7893;i th&agrave;nh c&ocirc;ng</strong>.</p>
            <div class="info-box">
                &#128336; Th&#7901;i gian: <strong>{DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC</strong><br/>
                &#127758; &#272;&#7883;a ch&#7881; IP: <strong>{ipAddress}</strong>
            </div>
            <p>&#9888;&#65039; N&#7871;u b&#7841;n <strong>kh&ocirc;ng th&#7921;c hi&#7879;n</strong> thao t&aacute;c n&agrave;y, t&#224;i kho&#7843;n c&#7911;a b&#7841;n c&oacute; th&#7875; &#273;&#227; b&#7883; x&acirc;m ph&#7841;m. Vui l&#242;ng li&ecirc;n h&#7879; h&#7895; tr&#7907; ngay.</p>
            <div style="text-align:center">
                <a href="{supportUrl}" class="btn">&#128682; Liên hệ hỗ trợ</a>
            </div>
            """);

    // ── 5. Đăng nhập từ IP / thiết bị lạ ─────────────────────────────────────
    public static string SuspiciousLogin(string fullName, string ipAddress, string userAgent, string blockUrl) =>
        BaseLayout("Cảnh báo đăng nhập từ thiết bị mới", "#DC2626", $"""
            <p>Xin ch&#224;o <strong>{fullName}</strong>,</p>
            <p>&#9888;&#65039; Ch&uacute;ng t&ocirc;i ph&aacute;t hi&#7879;n m&#7897;t l&#7847;n &#273;&#259;ng nh&#7853;p m&#7899;i v&agrave;o t&#224;i kho&#7843;n c&#7911;a b&#7841;n t&#7915; m&#7897;t thi&#7871;t b&#7883; ch&#432;a t&#7915;ng s&#7917; d&#7909;ng tr&#432;&#7899;c &#273;&acirc;y.</p>
            <div class="info-box">
                &#128336; Th&#7901;i gian: <strong>{DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC</strong><br/>
                &#127758; &#272;&#7883;a ch&#7881; IP: <strong>{ipAddress}</strong><br/>
                &#128187; Thi&#7871;t b&#7883;: <strong>{userAgent}</strong>
            </div>
            <p>N&#7871;u &#273;&acirc;y l&#224; <strong>b&#7841;n</strong>, h&#227;y b&#7887; qua email n&agrave;y.</p>
            <p>N&#7871;u <strong>kh&ocirc;ng ph&#7843;i b&#7841;n</strong>, h&#227;y b&#7843;o v&#7879; t&#224;i kho&#7843;n ngay l&#7853;p t&#7913;c:</p>
            <div style="text-align:center">
                <a href="{blockUrl}" class="btn">&#128274; Bảo vệ tài khoản ngay</a>
            </div>
            """);
    public static string OrderConfirm(
    string fullName,
    string orderCode,
    decimal amount,
    DateTime paidAt,
    List<string> itemNames) => $"""
    <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
        <h2 style="color:#2563eb">Xác nhận thanh toán thành công 🎉</h2>
        <p>Xin chào <strong>{fullName}</strong>,</p>
        <p>Đơn hàng của bạn đã được thanh toán thành công.</p>
        <table style="width:100%;border-collapse:collapse;margin:16px 0">
            <tr style="background:#f3f4f6">
                <td style="padding:8px;border:1px solid #e5e7eb">Mã đơn hàng</td>
                <td style="padding:8px;border:1px solid #e5e7eb"><strong>{orderCode}</strong></td>
            </tr>
            <tr>
                <td style="padding:8px;border:1px solid #e5e7eb">Số tiền</td>
                <td style="padding:8px;border:1px solid #e5e7eb"><strong>{amount:N0}đ</strong></td>
            </tr>
            <tr style="background:#f3f4f6">
                <td style="padding:8px;border:1px solid #e5e7eb">Thời gian</td>
                <td style="padding:8px;border:1px solid #e5e7eb">{paidAt:dd/MM/yyyy HH:mm}</td>
            </tr>
            <tr>
                <td style="padding:8px;border:1px solid #e5e7eb">Sản phẩm</td>
                <td style="padding:8px;border:1px solid #e5e7eb">{string.Join(", ", itemNames)}</td>
            </tr>
        </table>
        <p>Cảm ơn bạn đã mua hàng tại Qtemplate!</p>
    </div>
    """;

    public static string TicketReply(
        string fullName,
        string ticketCode,
        string subject,
        string replyMessage) => $"""
    <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
        <h2 style="color:#2563eb">Ticket của bạn có phản hồi mới 💬</h2>
        <p>Xin chào <strong>{fullName}</strong>,</p>
        <p>Ticket <strong>[{ticketCode}]</strong> - <em>{subject}</em> vừa nhận được phản hồi từ đội hỗ trợ:</p>
        <div style="background:#f3f4f6;padding:16px;border-left:4px solid #2563eb;margin:16px 0;border-radius:4px">
            {replyMessage}
        </div>
        <p>Đăng nhập để xem chi tiết và phản hồi tại Qtemplate.</p>
    </div>
    """;
    public static string TicketStatusChanged(
    string fullName, string ticketCode,
    string subject, string statusLabel) => $"""
    <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
        <h2 style="color:#2563eb">Cập nhật ticket hỗ trợ</h2>
        <p>Xin chào <strong>{fullName}</strong>,</p>
        <p>Ticket <strong>[{ticketCode}]</strong> - <em>{subject}</em> 
           hiện đang <strong>{statusLabel}</strong>.</p>
        <p>Đăng nhập để xem chi tiết tại Qtemplate.</p>
    </div>
    """;

    public static string ReviewReplied(
        string fullName, string reviewTitle, string reply) => $"""
    <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
        <h2 style="color:#2563eb">Review của bạn có phản hồi mới ⭐</h2>
        <p>Xin chào <strong>{fullName}</strong>,</p>
        <p>Review <em>"{reviewTitle}"</em> vừa nhận được phản hồi:</p>
        <div style="background:#f3f4f6;padding:16px;border-left:4px solid #2563eb;
                    margin:16px 0;border-radius:4px">
            {reply}
        </div>
    </div>
    """;

    public static string AccountLocked(string fullName, string? reason) => $"""
    <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
        <h2 style="color:#dc2626">Tài khoản của bạn đã bị khoá</h2>
        <p>Xin chào <strong>{fullName}</strong>,</p>
        <p>Tài khoản của bạn đã bị khoá{(string.IsNullOrEmpty(reason) ? "." : $" với lý do: <strong>{reason}</strong>.")}</p>
        <p>Nếu bạn cho rằng đây là nhầm lẫn, vui lòng liên hệ hỗ trợ.</p>
    </div>
    """;

    public static string AccountUnlocked(string fullName) => $"""
    <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
        <h2 style="color:#16a34a">Tài khoản của bạn đã được mở khoá ✅</h2>
        <p>Xin chào <strong>{fullName}</strong>,</p>
        <p>Tài khoản của bạn đã được mở khoá. Bạn có thể đăng nhập bình thường.</p>
    </div>
    """;

    public static string OrderCancelled(
        string fullName, string orderCode,
        decimal amount, string? reason) => $"""
    <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
        <h2 style="color:#dc2626">Đơn hàng đã bị hủy</h2>
        <p>Xin chào <strong>{fullName}</strong>,</p>
        <p>Đơn hàng <strong>{orderCode}</strong> ({amount:N0}đ) đã bị hủy
           {(string.IsNullOrEmpty(reason) ? "." : $" với lý do: <strong>{reason}</strong>.")}
        </p>
        <p>Nếu có thắc mắc vui lòng liên hệ hỗ trợ.</p>
    </div>
    """;
    public static string AccountSuspended(
    string fullName,
    string reason,
    string blockNote) =>
    BaseLayout("⚠ Tài khoản bị tạm khoá", "#DC2626", $"""
            <p>Xin chào <strong>{fullName}</strong>,</p>
            <p>Hệ thống bảo mật của chúng tôi đã phát hiện hoạt động bất thường
               và thực hiện khoá tạm thời tài khoản của bạn.</p>
            <div class="info-box">
                ⚠ <strong>Lý do:</strong> {reason}<br/>
                🕐 <strong>Trạng thái:</strong> {blockNote}
            </div>
            <p>Nếu bạn cho rằng đây là nhầm lẫn, vui lòng liên hệ đội ngũ hỗ trợ
               để được xem xét và mở khoá.</p>
            <p>Trân trọng,<br/><strong>Đội ngũ bảo mật Qtemplate</strong></p>
            """);
}