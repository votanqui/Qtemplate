using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.DTOs.Affiliate
{
    public class AdminAffiliateTransactionsDto
    {
        public int AffiliateId { get; set; }
        public string AffiliateCode { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public decimal CommissionRate { get; set; }
        public decimal TotalEarned { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public PaginatedResult<AffiliateTransactionDto> Transactions { get; set; } = new();
    }
}
