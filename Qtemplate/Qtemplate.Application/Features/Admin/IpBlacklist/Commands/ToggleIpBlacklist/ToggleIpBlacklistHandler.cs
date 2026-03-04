using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Admin.IpBlacklist.Commands.ToggleIpBlacklist
{
    public class ToggleIpBlacklistHandler
      : IRequestHandler<ToggleIpBlacklistCommand, ApiResponse<object>>
    {
        private readonly IIpBlacklistRepository _repo;
        public ToggleIpBlacklistHandler(IIpBlacklistRepository repo) => _repo = repo;

        public async Task<ApiResponse<object>> Handle(
            ToggleIpBlacklistCommand request, CancellationToken cancellationToken)
        {
            var entry = await _repo.GetByIdAsync(request.Id);
            if (entry is null)
                return ApiResponse<object>.Fail("Không tìm thấy IP");

            entry.IsActive = !entry.IsActive;
            await _repo.UpdateAsync(entry);

            return ApiResponse<object>.Ok(null!,
                entry.IsActive ? "Đã kích hoạt lại block" : "Đã tắt block IP");
        }
    }
}
