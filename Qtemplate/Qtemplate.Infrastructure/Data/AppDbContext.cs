using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;

namespace Qtemplate.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Auth
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Sản phẩm
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<TemplateImage> TemplateImages => Set<TemplateImage>();
    public DbSet<TemplateFeature> TemplateFeatures => Set<TemplateFeature>();
    public DbSet<TemplateVersion> TemplateVersions => Set<TemplateVersion>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TemplateTag> TemplateTags => Set<TemplateTag>();

    // Đơn hàng & Thanh toán
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Coupon> Coupons => Set<Coupon>();

    // Tương tác user
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<UserDownload> UserDownloads => Set<UserDownload>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // Hỗ trợ
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<TicketReply> TicketReplies => Set<TicketReply>();

    // Affiliate
    public DbSet<Affiliate> Affiliates => Set<Affiliate>();
    public DbSet<AffiliateTransaction> AffiliateTransactions => Set<AffiliateTransaction>();

    // Admin & Hệ thống
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

    // Bảo mật & Thống kê
    public DbSet<IpBlacklist> IpBlacklists => Set<IpBlacklist>();
    public DbSet<RequestLog> RequestLogs => Set<RequestLog>();
    public DbSet<Analytics> Analytics => Set<Analytics>();
    public DbSet<DailyStat> DailyStats => Set<DailyStat>();
    public DbSet<SecurityScanLog> SecurityScanLogs => Set<SecurityScanLog>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ======================================================
        // GLOBAL: Tắt cascade delete toàn bộ để tránh lỗi SQL Server
        // Chỉ bật lại cascade cho những chỗ thực sự cần bên dưới
        // ======================================================
        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // ======================================================
        // COMPOSITE KEY
        // ======================================================
        modelBuilder.Entity<TemplateTag>()
            .HasKey(tt => new { tt.TemplateId, tt.TagId });

        // ======================================================
        // INDEX
        // ======================================================
        modelBuilder.Entity<Wishlist>()
            .HasIndex(w => new { w.UserId, w.TemplateId }).IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email).IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(x => x.Token).IsUnique();

        modelBuilder.Entity<Category>()
            .HasIndex(x => x.Slug).IsUnique();

        modelBuilder.Entity<Template>()
            .HasIndex(x => x.Slug).IsUnique();

        modelBuilder.Entity<Coupon>()
            .HasIndex(x => x.Code).IsUnique();

        modelBuilder.Entity<Affiliate>()
            .HasIndex(x => x.AffiliateCode).IsUnique();

        modelBuilder.Entity<DailyStat>()
            .HasIndex(x => x.Date).IsUnique();

        modelBuilder.Entity<Setting>()
            .HasIndex(x => x.Key).IsUnique();

        modelBuilder.Entity<IpBlacklist>()
            .HasIndex(x => x.IpAddress).IsUnique();

        // ======================================================
        // USER
        // ======================================================
        modelBuilder.Entity<User>(e =>
        {
            e.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            e.Property(x => x.Email).HasMaxLength(255).IsRequired();
            e.Property(x => x.Role).HasMaxLength(50).HasDefaultValue("Customer");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ======================================================
        // REFRESH TOKEN
        // ======================================================
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.Property(x => x.Token).HasMaxLength(500).IsRequired();
        });

        // ======================================================
        // CATEGORY
        // ======================================================
        modelBuilder.Entity<Category>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(255).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(255).IsRequired();
        });

        // ======================================================
        // TEMPLATE
        // ======================================================
        modelBuilder.Entity<Template>(e =>
        {
            e.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            e.Property(x => x.Name).HasMaxLength(255).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(255).IsRequired();
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.Property(x => x.SalePrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.Status).HasMaxLength(50).HasDefaultValue("Draft");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ======================================================
        // ORDER
        // ======================================================
        modelBuilder.Entity<Order>(e =>
        {
            e.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            e.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.FinalAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Status).HasMaxLength(50).HasDefaultValue("Pending");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ======================================================
        // ORDER ITEM
        // ======================================================
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.Property(x => x.OriginalPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
        });

        // ======================================================
        // PAYMENT
        // ======================================================
        modelBuilder.Entity<Payment>(e =>
        {
            e.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Status).HasMaxLength(50).HasDefaultValue("Pending");
            e.HasOne(x => x.Order)
             .WithOne(x => x.Payment)
             .HasForeignKey<Payment>(x => x.OrderId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ======================================================
        // COUPON
        // ======================================================
        modelBuilder.Entity<Coupon>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Value).HasColumnType("decimal(18,2)");
            e.Property(x => x.MinOrderAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.MaxDiscountAmount).HasColumnType("decimal(18,2)");
        });

        // ======================================================
        // AFFILIATE
        // ======================================================
        modelBuilder.Entity<Affiliate>(e =>
        {
            e.Property(x => x.AffiliateCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.CommissionRate).HasColumnType("decimal(5,2)");
            e.Property(x => x.TotalEarned).HasColumnType("decimal(18,2)");
            e.Property(x => x.PendingAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.PaidAmount).HasColumnType("decimal(18,2)");
        });

        // ======================================================
        // AFFILIATE TRANSACTION
        // ======================================================
        modelBuilder.Entity<AffiliateTransaction>(e =>
        {
            e.Property(x => x.OrderAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Commission).HasColumnType("decimal(18,2)");
        });

        // ======================================================
        // DAILY STAT
        // ======================================================
        modelBuilder.Entity<DailyStat>(e =>
        {
            e.Property(x => x.TotalRevenue).HasColumnType("decimal(18,2)");
        });

        // ======================================================
        // SETTING
        // ======================================================
        modelBuilder.Entity<Setting>(e =>
        {
            e.Property(x => x.Key).HasMaxLength(100).IsRequired();
        });

        // ======================================================
        // IP BLACKLIST
        // ======================================================
        modelBuilder.Entity<IpBlacklist>(e =>
        {
            e.Property(x => x.IpAddress).HasMaxLength(50).IsRequired();
        });
        modelBuilder.Entity<SecurityScanLog>(e =>
        {
            e.HasIndex(x => new { x.Violation, x.IpAddress, x.UserId, x.ScannedAt });
            e.Property(x => x.Violation).HasMaxLength(50).IsRequired();
            e.Property(x => x.Action).HasMaxLength(50).IsRequired();
            e.Property(x => x.IpAddress).HasMaxLength(50);
        });
    }
}