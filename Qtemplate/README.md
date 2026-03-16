
Qtemplate Backend API
Nền tảng mua bán template thiết kế / web. Backend .NET 8, Clean Architecture, CQRS.

Tech Stack
Framework.NET 8 / ASP.NET CoreORMEntity Framework Core 8 + SQL ServerCQRSMediatR 12 + FluentValidationQueueMassTransit + RabbitMQReal-timeSignalRAuthJWT Bearer + Refresh TokenCacheIMemoryCache + Output Cache (.NET 8)AIOpenAI API (moderation review/ticket)Thanh toánSePay webhook

Cấu trúc project
Qtemplate.sln
├── Qtemplate/                  # Controllers, Middleware, Program.cs
├── Qtemplate.Application/      # CQRS Handlers, DTOs, Validators, Interfaces
├── Qtemplate.Infrastructure/   # EF, Repositories, Services, Background jobs
└── Qtemplate.domain/           # Entities, Repository Interfaces, Enums
Luồng request:
HTTP → Middleware stack → Controller → MediatR → ValidationBehavior → Handler → Repository → DB

Cài đặt
Yêu cầu: .NET 8 SDK, SQL Server, RabbitMQ
bash# 1. Clone
git clone <repo-url> && cd Qtemplate

# 2. Cấu hình (xem phần Cấu hình bên dưới)
cp Qtemplate/appsettings.json Qtemplate/appsettings.Development.json

# 3. RabbitMQ (Docker)
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management

# 4. Migrate DB
dotnet ef database update --project Qtemplate.Infrastructure --startup-project Qtemplate

# 5. Chạy
dotnet run --project Qtemplate
Swagger: http://localhost:5000/swagger (chỉ ở Development)

Cấu hình
Các key bắt buộc phải thay trong appsettings.Development.json:
json{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=QtemplateDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Max Pool Size=200;Min Pool Size=5;Connection Timeout=30"
  },
  "Jwt": {
    "SecretKey": "<chuỗi ngẫu nhiên ít nhất 32 ký tự>",
    "Issuer": "Qtemplate",
    "Audience": "Qtemplate",
    "AccessTokenExpiryMinutes": "15"
  },
  "Email": {
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": "587",
      "Username": "<your@gmail.com>",
      "Password": "<16-char App Password>"
    }
  },
  "App": {
    "BaseUrl": "http://localhost:5000"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}

⚠️ Không commit file chứa secret thực. Dùng biến môi trường hoặc secret manager khi deploy production.


API
Base URL: https://yourdomain.com
Auth header: Authorization: Bearer <access_token>
Public
MethodEndpointAuthMô tảPOST/api/auth/login—Đăng nhậpPOST/api/auth/register—Đăng kýPOST/api/auth/refreshtoken—Làm mới access tokenPOST/api/auth/logout✓Đăng xuấtPOST/api/auth/forgotpassword—Gửi email reset mật khẩuPOST/api/auth/resetpassword—Đặt lại mật khẩuPOST/api/auth/changepassword✓Đổi mật khẩuGET/api/auth/verifyemail?token=—Xác thực emailPOST/api/auth/resendverifyemail—Gửi lại email xác thựcGET/api/templates—Danh sách template (filter + phân trang)GET/api/templates/on-sale—Template đang saleGET/api/templates/{slug}—Chi tiết templateGET/api/templates/{slug}/download✓Tải template (phải đã mua)GET/api/categories—Danh mục (dạng cây cha-con)GET/api/tags—TagsGET/api/banners—Banner đang activePOST/api/orders✓Tạo đơn hàngGET/api/orders/{id}✓Chi tiết đơn hàngPOST/api/orders/apply-coupon✓Áp mã giảm giáPOST/api/orders/{id}/payment✓Tạo QR thanh toán SePayPOST/api/orders/{id}/cancel✓Huỷ đơnGET/api/orders/{id}/payment-status✓Kiểm tra trạng thái thanh toánGET/api/user/profile✓Thông tin cá nhânPUT/api/user/profile✓Cập nhật profilePUT/api/user/avatar✓Đổi avatarDELETE/api/user/account✓Xóa tài khoảnGET/api/user/purchases✓Lịch sử mua hàngGET/api/user/downloads✓Lịch sử tải vềPOST/api/user/wishlist/{templateId}✓Toggle wishlistGET/api/user/notifications✓Thông báoPATCH/api/user/notifications/{id}/read✓Đánh dấu đã đọcGET/api/templates/{slug}/reviews—Reviews của templatePOST/api/templates/{slug}/reviews✓Viết review (phải đã mua)GET/api/tickets✓Ticket hỗ trợ của tôiPOST/api/tickets✓Tạo ticketPOST/api/tickets/{id}/reply✓Trả lời ticketPOST/api/affiliate/register✓Đăng ký affiliateGET/api/affiliate/stats✓Thống kê hoa hồngGET/api/affiliate/transactions✓Lịch sử hoa hồngPOST/api/payments/sepay-callbackAPI KeyWebhook SePayGET/api/preview/{templateId}/{**filePath}—Preview template
Query params GET /api/templates: search, categorySlug, tagSlug, isFree, minPrice, maxPrice, onSale, isFeatured, isNew, techStack, sortBy (newest/popular/rating/price-asc/price-desc/discount), page, pageSize
Admin (yêu cầu role Admin)
NhómEndpointsTemplatesCRUD + publish, sale, pricing, thumbnail, preview, images, versions, bulk-saleUsersDanh sách, chi tiết, block/unblock, đổi role, xem đơn hàngOrdersDanh sách, chi tiết, cancel, cập nhật statusCategories / TagsCRUDBannersCRUD + upload ảnhCouponsCRUD — type: Percent / FixedMediaUpload, link URL, set file tải về, xóaSettingsCRUD key-value config runtimeIP BlacklistThêm, toggle, xóaReviewsDuyệt, reply, xóaTicketsReply, đổi status, assign, đổi priorityNotificationsGửi tới user hoặc broadcastAffiliateDuyệt đăng ký, đánh dấu đã trả hoa hồngLogsRequest logs, audit logs, email logs, refresh tokensStatsDashboard, orders, payments, coupons, analytics, media, security, dailyWishlistsXem danh sách, top template được wishlist
Response format
json{ "success": true, "message": "...", "data": { ... } }
Phân trang: data chứa { items, totalCount, page, pageSize, totalPages, hasNext, hasPrev }

Rate Limiting
EndpointGiới hạnChu kỳTất cả *100 req1 phútPOST /api/auth/login5 req1 phútPOST /api/auth/register3 req1 phútPOST /api/auth/forgotpassword3 req1 phútGET /api/preview/*200 req1 phút

Background Services
ServiceChu kỳMô tảRequestLogDrainService2 giâyFlush request log buffer vào DBAuditLogDrainService3 giâyFlush audit log buffer vào DBSuspiciousBehaviorBackgroundService30 giâyQuét hành vi bất thường, tự block IP/userAiModerationBackgroundService30 giâyAI duyệt review và ticket mớiOrderPaymentReminderService1 phútNhắc thanh toán đơn pendingEmailRetryBackgroundService2 phútRetry email thất bạiAutoUnblockService5 phútMở khóa user hết thời gian blockCouponAutoDisableService15 phútTắt coupon hết hạnDailyStatAggregationService00:00 UTCTổng hợp thống kê ngàyRefreshTokenCleanupService02:00 UTCXóa refresh token hết hạnAffiliateCommissionService03:00 UTCTự duyệt hoa hồng affiliate

Database
SQL Server, 31 bảng, quản lý bằng EF Core migrations.
bashdotnet ef migrations add <TênMigration> --project Qtemplate.Infrastructure --startup-project Qtemplate
dotnet ef database update --project Qtemplate.Infrastructure --startup-project Qtemplate
NhómBảngAuthUsers, RefreshTokensTemplatesTemplates, TemplateImages, TemplateVersions, TemplateFeatures, TemplateTagsTaxonomyCategories, Tags, MediaFilesCommerceOrders, OrderItems, Payments, CouponsAffiliateAffiliates, AffiliateTransactionsSocialReviews, Wishlists, SupportTickets, TicketReplies, Notifications, UserDownloadsAnalyticsAnalytics, DailyStatsLogsRequestLogs, AuditLogs, EmailLogs, SecurityScanLogsConfigSettings, IpBlacklists, Banners
Settings cấu hình qua DB (admin sửa trực tiếp, không cần restart server):
sepay.api_key · openai.api_key · openai.model · affiliate.commission_rate · affiliate.auto_approve_days · order.reminder_enabled · order.payment_reminder_minutes · order.auto_cancel_minutes · security.time_window_minutes · security.max_requests_per_window · security.max_failed_logins · auth.refresh_token_retention_days
