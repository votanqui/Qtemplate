using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Admin.IpBlacklist.Commands.ToggleIpBlacklist;

public class ToggleIpBlacklistCommand : IRequest<ApiResponse<object>>
{
    public int Id { get; set; }
    public string? AdminEmail { get; set; }  // ← thêm để ghi vào OverrideByEmail
    public string? Note { get; set; }  // ← ghi chú lý do mở khoá
}