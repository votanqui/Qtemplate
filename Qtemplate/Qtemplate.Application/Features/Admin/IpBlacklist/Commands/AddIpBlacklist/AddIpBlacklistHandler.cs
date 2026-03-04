using MediatR;
using Qtemplate.Application.DTOs.IpBlacklist;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Admin.IpBlacklist.Commands.AddIpBlacklist
{
    public class AddIpBlacklistHandler
       : IRequestHandler<AddIpBlacklistCommand, ApiResponse<IpBlacklistDto>>
    {
        private readonly IIpBlacklistRepository _repo;

        public AddIpBlacklistHandler(IIpBlacklistRepository repo) => _repo = repo;

        public async Task<ApiResponse<IpBlacklistDto>> Handle(
            AddIpBlacklistCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.IpAddress))
                return ApiResponse<IpBlacklistDto>.Fail("IP Address không được để trống");

            var existing = await _repo.GetByIpAsync(request.IpAddress);
            if (existing is not null && existing.IsActive)
                return ApiResponse<IpBlacklistDto>.Fail("IP này đã bị block");

            var entry = new Domain.Entities.IpBlacklist
            {
                IpAddress = request.IpAddress.Trim(),
                Reason = request.Reason?.Trim(),
                Type = "Manual",
                BlockedBy = request.AdminEmail,
                IsActive = true,
                ExpiredAt = request.ExpiredAt,
                BlockedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entry);
            return ApiResponse<IpBlacklistDto>.Ok(ToDto(entry), "Đã block IP thành công");
        }

        internal static IpBlacklistDto ToDto(Domain.Entities.IpBlacklist e) => new()
        {
            Id = e.Id,
            IpAddress = e.IpAddress,
            Reason = e.Reason,
            Type = e.Type,
            BlockedBy = e.BlockedBy,
            IsActive = e.IsActive,
            ExpiredAt = e.ExpiredAt,
            BlockedAt = e.BlockedAt
        };
    }
}
