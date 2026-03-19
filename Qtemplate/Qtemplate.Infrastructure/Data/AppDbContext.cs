// File: Qtemplate.Infrastructure/Data/AppDbContext.cs
//
// THAY ĐỔI SO VỚI BẢN CŨ:
//   Thêm các index còn thiếu vào section "INDEX — Performance":
//   - Analytics.CreatedAt, Analytics.PageUrl, Analytics.IpAddress   (range query + groupby)
//   - RequestLog.StatusCode                                          (filter theo status)
//   - Template.Status, Template.CategoryId                          (public list filter)
//   - Template.IsFeatured, Template.IsNew, Template.SalesCount      (sort/filter hot nhất)
//   - Payment.CreatedAt, Payment.Status, Payment.OrderId            (stats queries)
//   - Order.CouponCode (partial index, chỉ khi NOT NULL)            (coupon usage query)
//   - IpBlacklist.IsActive                                          (middleware filter)
//   - EmailLog.CreatedAt, EmailLog.Status                           (paged query)

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

    // Bảng tin / Tin tức
    public DbSet<Post> Posts => Set<Post>();
      public DbSet<CommunityPost>    CommunityPosts    => Set<CommunityPost>();
      public DbSet<CommunityComment> CommunityComments => Set<CommunityComment>();
      public DbSet<CommunityLike>    CommunityLikes    => Set<CommunityLike>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ======================================================
        // GLOBAL: Tắt cascade delete toàn bộ để tránh lỗi SQL Server
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
        // INDEX — Unique constraints
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
        // INDEX — Performance (các cột query thường xuyên)
        // ======================================================

        // RefreshToken — login, renew, revoke, cleanup
        modelBuilder.Entity<RefreshToken>()
            .HasIndex(x => x.UserId);
        modelBuilder.Entity<RefreshToken>()
            .HasIndex(x => new { x.IsRevoked, x.ExpiresAt });

        // AuditLog — admin filter theo thời gian / email / action
        modelBuilder.Entity<AuditLog>()
            .HasIndex(x => x.CreatedAt);
        modelBuilder.Entity<AuditLog>()
            .HasIndex(x => x.UserEmail);
        modelBuilder.Entity<AuditLog>()
            .HasIndex(x => x.Action);

        // RequestLog — stats, security scanner, paged admin view
        modelBuilder.Entity<RequestLog>()
            .HasIndex(x => x.CreatedAt);
        modelBuilder.Entity<RequestLog>()
            .HasIndex(x => x.IpAddress);

        // Notification — user inbox, unread count
        modelBuilder.Entity<Notification>()
            .HasIndex(x => new { x.UserId, x.IsRead });
        modelBuilder.Entity<Notification>()
            .HasIndex(x => x.CreatedAt);

        // Order — stats, reminder service, admin list
        modelBuilder.Entity<Order>()
            .HasIndex(x => new { x.Status, x.CreatedAt });
        modelBuilder.Entity<Order>()
            .HasIndex(x => x.UserId);
        // THÊM MỚI: coupon usage query — partial index (chỉ các row có CouponCode)
        modelBuilder.Entity<Order>()
            .HasIndex(x => x.CouponCode)
            .HasFilter("[CouponCode] IS NOT NULL");

        // AffiliateTransaction — auto-approve service, paged query
        modelBuilder.Entity<AffiliateTransaction>()
            .HasIndex(x => new { x.Status, x.CreatedAt });
        modelBuilder.Entity<AffiliateTransaction>()
            .HasIndex(x => x.AffiliateId);

        // THÊM MỚI: Analytics — range query là query chính của GetAnalyticsStatsHandler
        modelBuilder.Entity<Analytics>()
            .HasIndex(x => x.CreatedAt);
        // composite cho filter theo IP + thời gian (security + analytics)
        modelBuilder.Entity<Analytics>()
            .HasIndex(x => new { x.CreatedAt, x.IpAddress });
        // groupby theo PageUrl
        modelBuilder.Entity<Analytics>()
            .HasIndex(x => x.PageUrl);

        // THÊM MỚI: Template — public list filter/sort hottest
        modelBuilder.Entity<Template>()
            .HasIndex(x => x.Status);                          // WHERE Status = 'Published'
        modelBuilder.Entity<Template>()
            .HasIndex(x => x.CategoryId);                     // WHERE CategoryId = ?
        modelBuilder.Entity<Template>()
            .HasIndex(x => new { x.Status, x.CategoryId });   // composite (published + category)
        modelBuilder.Entity<Template>()
            .HasIndex(x => x.IsFeatured);                     // isFeatured filter
        modelBuilder.Entity<Template>()
            .HasIndex(x => x.IsNew);                          // isNew filter
        modelBuilder.Entity<Template>()
            .HasIndex(x => x.SalesCount);                     // ORDER BY popular
        modelBuilder.Entity<Template>()
            .HasIndex(x => x.CreatedAt);                      // ORDER BY newest (default)

        // THÊM MỚI: Payment — stats queries filter theo thời gian và status
        modelBuilder.Entity<Payment>()
            .HasIndex(x => x.CreatedAt);
        modelBuilder.Entity<Payment>()
            .HasIndex(x => x.Status);
        modelBuilder.Entity<Payment>()
            .HasIndex(x => new { x.Status, x.CreatedAt });    // composite cho stats range query

        // THÊM MỚI: IpBlacklist — middleware filter IsActive (rất nhiều lần gọi)
        modelBuilder.Entity<IpBlacklist>()
            .HasIndex(x => x.IsActive);
        modelBuilder.Entity<IpBlacklist>()
            .HasIndex(x => new { x.IsActive, x.ExpiredAt });  // composite cho cache refresh query

        // THÊM MỚI: EmailLog — paged admin query + filter
        modelBuilder.Entity<EmailLog>()
            .HasIndex(x => x.CreatedAt);
        modelBuilder.Entity<EmailLog>()
            .HasIndex(x => x.Status);

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

        // ======================================================
        // SECURITY SCAN LOG
        // ======================================================
        modelBuilder.Entity<SecurityScanLog>(e =>
        {
            e.HasIndex(x => new { x.Violation, x.IpAddress, x.UserId, x.ScannedAt });
            e.Property(x => x.Violation).HasMaxLength(50).IsRequired();
            e.Property(x => x.Action).HasMaxLength(50).IsRequired();
            e.Property(x => x.IpAddress).HasMaxLength(50);
        });

        // ======================================================
        // POST (Bảng tin / Tin tức)
        // ======================================================
        modelBuilder.Entity<Post>(e =>
        {
            // Unique slug
            e.HasIndex(p => p.Slug).IsUnique();

            // Index cho public list: lọc theo Status + sắp xếp theo PublishedAt
            e.HasIndex(p => new { p.Status, p.PublishedAt });

            // Index cho lọc bài nổi bật
            e.HasIndex(p => p.IsFeatured);

            // Giới hạn độ dài cột
            e.Property(p => p.Title).HasMaxLength(300).IsRequired();
            e.Property(p => p.Slug).HasMaxLength(300).IsRequired();
            e.Property(p => p.Excerpt).HasMaxLength(500);
            e.Property(p => p.ThumbnailUrl).HasMaxLength(500);
            e.Property(p => p.Status).HasMaxLength(20).HasDefaultValue("Draft");
            e.Property(p => p.AuthorId).HasMaxLength(100).IsRequired();
            e.Property(p => p.AuthorName).HasMaxLength(200).IsRequired();
            e.Property(p => p.Tags).HasMaxLength(500);
            e.Property(p => p.MetaTitle).HasMaxLength(300);
            e.Property(p => p.MetaDescription).HasMaxLength(500);
            e.Property(p => p.ViewCount).HasDefaultValue(0);
        });
               modelBuilder.Entity<CommunityPost>(e =>
               {
                   e.HasIndex(p => p.UserId);
                   e.HasIndex(p => p.CreatedAt);
                   e.HasIndex(p => p.IsHidden);
                   e.Property(p => p.Content).HasMaxLength(3000).IsRequired();
                   e.Property(p => p.ImageUrl).HasMaxLength(500);
                   e.Property(p => p.HideReason).HasMaxLength(500);
                  e.Property(p => p.LikeCount).HasDefaultValue(0);
                  e.Property(p => p.CommentCount).HasDefaultValue(0);
                e.Property(p => p.IsHidden).HasDefaultValue(false);
              });
        
               modelBuilder.Entity<CommunityLike>(e =>
                {
                   // Unique: 1 user chỉ like 1 bài 1 lần
                   e.HasIndex(l => new { l.PostId, l.UserId }).IsUnique();
               });
        
               modelBuilder.Entity<CommunityComment>(e =>
              {
                   e.HasIndex(c => c.PostId);
                   e.HasIndex(c => c.UserId);
                   e.HasIndex(c => c.ParentId);
                   e.HasIndex(c => c.IsHidden);
                  e.Property(c => c.Content).HasMaxLength(1000).IsRequired();
                  e.Property(c => c.IsHidden).HasDefaultValue(false);
       
                 // Self-referencing FK cho replies (restrict để tránh cascade)
                   e.HasOne(c => c.Parent)
                    .WithMany(c => c.Replies)
                   .HasForeignKey(c => c.ParentId)
                   .OnDelete(DeleteBehavior.Restrict);
              });
    }
}