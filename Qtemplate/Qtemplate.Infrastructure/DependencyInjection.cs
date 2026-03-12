using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;
using Qtemplate.Infrastructure.Repositories;
using Qtemplate.Infrastructure.Services;
using Qtemplate.Infrastructure.Services.AuditLog;
using Qtemplate.Infrastructure.Services.Auth;
using Qtemplate.Infrastructure.Services.Email;
using Qtemplate.Infrastructure.Services.FileUpload;
using MassTransit;
using Qtemplate.Infrastructure.Services.Email;
using Qtemplate.Infrastructure.Services.Notification;
using Qtemplate.Infrastructure.Services.Security;
namespace Qtemplate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // ── Auth ─────────────────────────────────────────────────────────────
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // ── User ─────────────────────────────────────────────────────────────
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUserDownloadRepository, UserDownloadRepository>();
        services.AddScoped<IWishlistRepository, WishlistRepository>();

        // ── Template ─────────────────────────────────────────────────────────
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITemplateImageRepository, TemplateImageRepository>();
        services.AddScoped<ITemplateVersionRepository, TemplateVersionRepository>();
        services.AddScoped<IMediaFileRepository, MediaFileRepository>();

        // ── Order / Payment / Coupon ──────────────────────────────────────────
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();

        // ── Review ───────────────────────────────────────────────────────────
        services.AddScoped<IReviewRepository, ReviewRepository>();

        // ── Ticket ───────────────────────────────────────────────────────────
        services.AddScoped<ITicketRepository, TicketRepository>();

        // ── Banner / Affiliate ────────────────────────────────────────────────
        services.AddScoped<IBannerRepository, BannerRepository>();
        services.AddScoped<IAffiliateRepository, AffiliateRepository>();

        // ── Settings / Analytics ──────────────────────────────────────────────
        services.AddScoped<ISettingRepository, SettingRepository>();
        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

        // ── Stats (thay IStatsService) ────────────────────────────────────────
        services.AddScoped<IStatsRepository, StatsRepository>();

        // ── Security / Logs ───────────────────────────────────────────────────
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IIpBlacklistRepository, IpBlacklistRepository>();
        // IRequestLogRepository, IEmailLogRepository đã gộp vào IStatsRepository
        // Giữ lại nếu vẫn dùng trực tiếp ở middleware/service khác
        services.AddScoped<IRequestLogRepository, RequestLogRepository>();
        services.AddScoped<IEmailLogRepository, EmailLogRepository>();

        // ── Services ──────────────────────────────────────────────────────────
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IFileUploadService, FileUploadService>();
        services.AddScoped<INotificationService, NotificationService>();
        // security
        services.AddScoped<SuspiciousBehaviorScanner>();
        services.AddHostedService<SuspiciousBehaviorBackgroundService>();
        services.AddScoped<ISecurityScanLogRepository, SecurityScanLogRepository>();
        // ── AI Moderation ─────────────────────────────────────────────────────
        services.AddHttpClient<IAiModerationService, AiModerationService>();
        services.AddHostedService<AiModerationBackgroundService>();
        // Email
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddHostedService<EmailRetryBackgroundService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<EmailConsumer>();
            x.AddConsumer<EmailConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");

                cfg.Host(rabbit["Host"], rabbit["VHost"], h =>
                {
                    h.Username(rabbit["Username"]!);
                    h.Password(rabbit["Password"]!);
                });

                cfg.ReceiveEndpoint("email-queue", e =>
                {
                    e.ConfigureConsumer<EmailConsumer>(ctx);

                    // Retry: 5s → 10s → 30s
                    e.UseMessageRetry(r => r.Intervals(
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(30)));
                });
            });
        });
        return services;
    }
}