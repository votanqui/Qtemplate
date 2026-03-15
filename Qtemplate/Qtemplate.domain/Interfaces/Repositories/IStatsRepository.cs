// File: Qtemplate.domain/Interfaces/Repositories/IStatsRepository.cs
//
// THAY ĐỔI: GetPaidOrdersAsync() thêm tham số tùy chọn from/to
// để tránh load toàn bộ bảng Orders không giới hạn.

using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface IStatsRepository
{
    // Order
    Task<List<Order>> GetOrdersInRangeAsync(DateTime from, DateTime to, bool includeItems = false);

    // THAY ĐỔI: thêm from/to optional — caller nên luôn truyền để tránh full table scan
    Task<List<Order>> GetPaidOrdersAsync(DateTime? from = null, DateTime? to = null);

    // Payment
    Task<List<Payment>> GetPaymentsInRangeAsync(DateTime from, DateTime to, bool includeOrder = false);

    // Coupon
    Task<List<Coupon>> GetAllCouponsAsync();
    Task<List<(string Code, decimal TotalDiscount)>> GetCouponUsageAsync();

    // Analytics
    Task<List<Analytics>> GetAnalyticsInRangeAsync(DateTime from, DateTime to);

    // IpBlacklist
    Task<List<IpBlacklist>> GetIpBlacklistPagedAsync(int page, int pageSize);
    Task<int> CountIpBlacklistAsync();

    // RequestLog
    Task<List<RequestLog>> GetRequestLogsPagedAsync(
        string? ip, string? userId, string? endpoint, int? statusCode,
        int page, int pageSize);
    Task<int> CountRequestLogsAsync(
        string? ip, string? userId, string? endpoint, int? statusCode);

    // EmailLog
    Task<List<EmailLog>> GetEmailLogsPagedAsync(string? status, string? template, int page, int pageSize);
    Task<int> CountEmailLogsAsync(string? status, string? template);

    // Security stats
    Task<List<IpBlacklist>> GetAllIpBlacklistAsync();
    Task<List<RequestLog>> GetRequestLogsInRangeAsync(DateTime from, DateTime to);
    Task<List<EmailLog>> GetAllEmailLogsAsync();

    // Daily Stats
    Task<List<DailyStat>> GetDailyStatsAsync(DateTime from, DateTime to);
}