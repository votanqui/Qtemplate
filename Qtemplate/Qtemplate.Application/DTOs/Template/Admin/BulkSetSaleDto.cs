using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.DTOs.Template.Admin
{
    public class BulkSetSaleDto
    {
        public List<Guid> TemplateIds { get; set; } = new();
        public decimal? SalePrice { get; set; }   // null = xóa sale
        public DateTime? SaleStartAt { get; set; }
        public DateTime? SaleEndAt { get; set; }
    }
}
