// DTOs/Template/Admin/SetSaleDto.cs
namespace Qtemplate.Application.DTOs.Template.Admin;

public class SetSaleDto
{
    public decimal? SalePrice { get; set; }  // null = xóa sale
    public DateTime? SaleStartAt { get; set; }
    public DateTime? SaleEndAt { get; set; }
}