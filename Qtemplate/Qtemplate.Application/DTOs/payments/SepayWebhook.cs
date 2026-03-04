using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.DTOs.payments
{
    public class SepayWebhookRequest
    {
        public string Id { get; set; } = string.Empty;
        public string? Gateway { get; set; }
        public string? TransactionDate { get; set; }
        public string? AccountNumber { get; set; }
        public string? SubAccount { get; set; }
        public string? Code { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? TransferType { get; set; }
        public decimal TransferAmount { get; set; }
        public decimal Accumulated { get; set; }
        public string? ReferenceCode { get; set; }
        public string? Description { get; set; }
    }


}
