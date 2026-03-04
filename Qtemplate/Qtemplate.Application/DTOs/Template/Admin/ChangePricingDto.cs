// DTOs/Template/Admin/ChangePricingDto.cs
namespace Qtemplate.Application.DTOs.Template.Admin;

public class ChangePricingDto
{
    public bool IsFree { get; set; }
    public decimal Price { get; set; }  // bỏ qua nếu IsFree = true
}