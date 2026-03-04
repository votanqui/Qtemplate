using Qtemplate.Application.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.DTOs.payments
{
    public class PaymentStatusDto
    {
        public string OrderCode { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal FinalAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public string? BankCode { get; set; }
        public DateTime? PaidAt { get; set; }
        public bool IsPaid { get; set; }
        public List<OrderItemDto> Downloads { get; set; } = new();
    }
}
