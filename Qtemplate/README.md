# Qtemplate Backend API

Nền tảng mua bán template thiết kế / web. Backend .NET 8, Clean Architecture, CQRS.

---

## Tech Stack

| | |
|---|---|
| Framework | .NET 8 / ASP.NET Core |
| ORM | Entity Framework Core 8 + SQL Server |
| CQRS | MediatR 12 + FluentValidation |
| Queue | MassTransit + RabbitMQ |
| Real-time | SignalR |
| Auth | JWT Bearer + Refresh Token |
| Cache | IMemoryCache + Output Cache (.NET 8) |
| AI | OpenAI API (moderation review/ticket) |
| Thanh toán | SePay webhook |

---

## Cấu trúc project

```
Qtemplate.sln
├── Qtemplate/                  # Controllers, Middleware, Program.cs
├── Qtemplate.Application/      # CQRS Handlers, DTOs, Validators, Interfaces
├── Qtemplate.Infrastructure/   # EF, Repositories, Services, Background jobs
└── Qtemplate.domain/           # Entities, Repository Interfaces, Enums
```

Luồng request:

```
HTTP → Middleware stack → Controller → MediatR → ValidationBehavior → Handler → Repository → DB
```

---

## Cài đặt

**Yêu cầu:** .NET 8 SDK, SQL Server, RabbitMQ

```bash
# 1. Clone
git clone <repo-url> && cd Qtemplate

# 2. Cấu hình
cp Qtemplate/appsettings.json Qtemplate/appsettings.Development.json
# Điền các giá trị thực vào file vừa tạo (xem phần Cấu hình bên dưới)

# 3. RabbitMQ qua Docker
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management

# 4. Migrate DB
dotnet ef database update --project Qtemplate.Infrastructure --startup-project Qtemplate

# 5. Chạy
dotnet run --project Qtemplate
```

Swagger UI: `http://localhost:5000/swagger` — chỉ khả dụng ở môi trường Development.

---

## Cấu hình

Điền vào `appsettings.Development.json`:

```json
{
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
```

> ⚠️ Không commit file chứa secret thực. Dùng biến môi trường hoặc secret manager khi deploy production.

---

## API

Base URL: `https://yourdomain.com`

Auth header: `Authorization: Bearer <access_token>`

### Auth — `/api/auth`

| Method | Endpoint | Auth | Mô tả |
|---|---|---|---|
| POST | `/api/auth/login` | — | Đăng nhập, trả access + refresh token |
| POST | `/api/auth/register` | — | Đăng ký tài khoản |
| POST | `/api/auth/refreshtoken` | — | Làm mới access token |
| POST | `/api/auth/logout` | ✓ | Đăng xuất, revoke refresh token |
| POST | `/api/auth/forgotpassword` | — | Gửi email reset mật khẩu |
| POST | `/api/auth/resetpassword` | — | Đặt lại mật khẩu bằng token |
| POST | `/api/auth/changepassword` | ✓ | Đổi mật khẩu |
| GET | `/api/auth/verifyemail?token=` | — | Xác thực email sau đăng ký |
| POST | `/api/auth/resendverifyemail` | — | Gửi lại email xác thực |

### Templates — `/api/templates`

| Method | Endpoint | Auth | Mô tả |
|---|---|---|---|
| GET | `/api/templates` | — | Danh sách template, filter + phân trang |
| GET | `/api/templates/on-sale` | — | Template đang sale |
| GET | `/api/templates/{slug}` | — | Chi tiết template |
| GET | `/api/templates/{slug}/download` | ✓ | Tải template (phải đã mua) |

Query params: `search`, `categorySlug`, `tagSlug`, `isFree`, `minPrice`, `maxPrice`, `onSale`, `isFeatured`, `isNew`, `techStack`, `sortBy` (`newest` / `popular` / `rating` / `price-asc` / `price-desc` / `discount`), `page`, `pageSize`

### Orders — `/api/orders`

| Method | Endpoint | Auth | Mô tả |
|---|---|---|---|
| POST | `/api/orders` | ✓ | Tạo đơn hàng |
| GET | `/api/orders/{id}` | ✓ | Chi tiết đơn hàng |
| GET | `/api/orders/code/{orderCode}` | ✓ | Tìm theo mã đơn |
| POST | `/api/orders/apply-coupon` | ✓ | Tính giá sau khi áp mã giảm giá |
| POST | `/api/orders/{id}/payment` | ✓ | Tạo QR thanh toán SePay |
| POST | `/api/orders/{id}/cancel` | ✓ | Huỷ đơn |
| GET | `/api/orders/{id}/payment-status` | ✓ | Kiểm tra trạng thái thanh toán |

Order status: `Pending` → `Paid` / `Cancelled`

### User — `/api/user`

| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/user/profile` | Thông tin cá nhân |
| PUT | `/api/user/profile` | Cập nhật profile |
| PUT | `/api/user/avatar` | Đổi avatar (multipart/form-data) |
| DELETE | `/api/user/account` | Xóa tài khoản (soft delete) |
| GET | `/api/user/purchases` | Lịch sử mua hàng |
| GET | `/api/user/downloads` | Lịch sử tải về |
| POST | `/api/user/wishlist/{templateId}` | Toggle wishlist |
| GET | `/api/user/notifications` | Danh sách thông báo |
| PATCH | `/api/user/notifications/{id}/read` | Đánh dấu đã đọc |
| PATCH | `/api/user/notifications/read-all` | Đánh dấu tất cả đã đọc |

### Các endpoint khác

| Method | Endpoint | Auth | Mô tả |
|---|---|---|---|
| GET | `/api/categories` | — | Danh mục dạng cây cha-con |
| GET | `/api/tags` | — | Tags |
| GET | `/api/banners` | — | Banner đang active |
| GET | `/api/templates/{slug}/reviews` | — | Reviews của template |
| POST | `/api/templates/{slug}/reviews` | ✓ | Viết review (phải đã mua) |
| PUT | `/api/user/reviews/{id}` | ✓ | Sửa review |
| DELETE | `/api/user/reviews/{id}` | ✓ | Xóa review |
| GET | `/api/tickets` | ✓ | Ticket hỗ trợ của tôi |
| POST | `/api/tickets` | ✓ | Tạo ticket mới |
| POST | `/api/tickets/{id}/reply` | ✓ | Trả lời ticket |
| POST | `/api/affiliate/register` | ✓ | Đăng ký affiliate |
| GET | `/api/affiliate/stats` | ✓ | Thống kê hoa hồng |
| GET | `/api/affiliate/transactions` | ✓ | Lịch sử hoa hồng |
| POST | `/api/payments/sepay-callback` | API Key | Webhook SePay |
| GET | `/api/preview/{templateId}/{**filePath}` | — | Preview template |

### Admin (yêu cầu role `Admin`)

| Nhóm | Endpoints chính |
|---|---|
| Templates | CRUD, publish, sale, pricing, thumbnail, preview, images, versions, bulk-sale |
| Users | Danh sách, chi tiết, block/unblock, đổi role, xem đơn hàng |
| Orders | Danh sách, chi tiết, cancel, cập nhật status |
| Categories / Tags | CRUD |
| Banners | CRUD + upload ảnh |
| Coupons | CRUD — type: `Percent` / `Fixed` |
| Media | Upload, link URL, set file tải về, xóa |
| Settings | CRUD key-value config runtime |
| IP Blacklist | Thêm, toggle, xóa |
| Reviews | Duyệt, reply, xóa |
| Tickets | Reply, đổi status, assign, đổi priority |
| Notifications | Gửi tới user hoặc broadcast all |
| Affiliate | Duyệt đăng ký, đánh dấu đã trả hoa hồng |
| Logs | Request logs, audit logs, email logs, refresh tokens |
| Stats | Dashboard, orders, payments, coupons, analytics, media, security, daily |
| Wishlists | Danh sách, top được wishlist nhiều nhất |

### Response format

```json
{
  "success": true,
  "message": "Thành công",
  "data": { ... }
}
```

Phân trang — `data` chứa: `items`, `totalCount`, `page`, `pageSize`, `totalPages`, `hasNext`, `hasPrev`

### SignalR

Endpoint: `wss://domain/hubs/notifications`

Auth: query string `?access_token=<jwt>` hoặc cookie `accessToken`

Event nhận từ server: `ReceiveNotification` → `{ id, title, message, type, redirectUrl, isRead, createdAt }`

---

## Rate Limiting

| Endpoint | Giới hạn | Chu kỳ |
|---|---|---|
| Tất cả `*` | 100 req | 1 phút |
| `POST /api/auth/login` | 5 req | 1 phút |
| `POST /api/auth/register` | 3 req | 1 phút |
| `POST /api/auth/forgotpassword` | 3 req | 1 phút |
| `GET /api/preview/*` | 200 req | 1 phút |

---

## Background Services

| Service | Chu kỳ | Mô tả |
|---|---|---|
| `RequestLogDrainService` | 2 giây | Flush request log buffer vào DB |
| `AuditLogDrainService` | 3 giây | Flush audit log buffer vào DB |
| `SuspiciousBehaviorBackgroundService` | 30 giây | Quét hành vi bất thường, tự block IP/user |
| `AiModerationBackgroundService` | 30 giây | AI duyệt review và ticket mới |
| `OrderPaymentReminderService` | 1 phút | Nhắc thanh toán đơn pending |
| `EmailRetryBackgroundService` | 2 phút | Retry email thất bại |
| `AutoUnblockService` | 5 phút | Mở khóa user hết thời gian block |
| `CouponAutoDisableService` | 15 phút | Tắt coupon hết hạn |
| `DailyStatAggregationService` | 00:00 UTC | Tổng hợp thống kê ngày |
| `RefreshTokenCleanupService` | 02:00 UTC | Xóa refresh token hết hạn |
| `AffiliateCommissionService` | 03:00 UTC | Tự duyệt hoa hồng affiliate |

---

## Database

SQL Server, 31 bảng, quản lý bằng EF Core migrations.

```bash
# Tạo migration mới
dotnet ef migrations add <TênMigration> --project Qtemplate.Infrastructure --startup-project Qtemplate

# Áp dụng
dotnet ef database update --project Qtemplate.Infrastructure --startup-project Qtemplate
```

| Nhóm | Bảng |
|---|---|
| Auth | `Users`, `RefreshTokens` |
| Templates | `Templates`, `TemplateImages`, `TemplateVersions`, `TemplateFeatures`, `TemplateTags` |
| Taxonomy | `Categories`, `Tags`, `MediaFiles` |
| Commerce | `Orders`, `OrderItems`, `Payments`, `Coupons` |
| Affiliate | `Affiliates`, `AffiliateTransactions` |
| Social | `Reviews`, `Wishlists`, `SupportTickets`, `TicketReplies`, `Notifications`, `UserDownloads` |
| Analytics | `Analytics`, `DailyStats` |
| Logs | `RequestLogs`, `AuditLogs`, `EmailLogs`, `SecurityScanLogs` |
| Config | `Settings`, `IpBlacklists`, `Banners` |

**Settings cấu hình qua DB** — admin sửa qua UI, không cần restart server:

`sepay.api_key` · `openai.api_key` · `openai.model` · `affiliate.commission_rate` · `affiliate.auto_approve_days` · `order.reminder_enabled` · `order.payment_reminder_minutes` · `order.auto_cancel_minutes` · `security.time_window_minutes` · `security.max_requests_per_window` · `security.max_failed_logins` · `auth.refresh_token_retention_days`

