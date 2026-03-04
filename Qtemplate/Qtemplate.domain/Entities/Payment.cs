namespace Qtemplate.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string? SepayCode { get; set; }
    public string? TransferContent { get; set; }            // Nội dung CK: "QT-20240101-XXXX"
    public string? BankCode { get; set; }                   // Ngân hàng chuyển
    public string? AccountNumber { get; set; }              // STK nhận
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? FailReason { get; set; }
    public string? RawCallback { get; set; }                // Raw JSON từ SePay
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Order Order { get; set; } = null!;
}