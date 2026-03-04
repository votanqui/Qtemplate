using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Tags.Commands.UpdateTag;

public class UpdateTagHandler : IRequestHandler<UpdateTagCommand, ApiResponse<object>>
{
    private readonly ITagRepository _tagRepo;
    private readonly IAuditLogService _auditLogService;

    public UpdateTagHandler(ITagRepository tagRepo, IAuditLogService auditLogService)
    {
        _tagRepo = tagRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<object>> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _tagRepo.GetByIdAsync(request.Id);
        if (tag is null)
            return ApiResponse<object>.Fail("Không tìm thấy tag");

        if (tag.Slug != request.Dto.Slug && await _tagRepo.SlugExistsAsync(request.Dto.Slug))
            return ApiResponse<object>.Fail("Slug đã tồn tại");

        var oldValues = new { tag.Name, tag.Slug };
        tag.Name = request.Dto.Name.Trim();
        tag.Slug = request.Dto.Slug.Trim().ToLower();

        await _tagRepo.UpdateAsync(tag);

        await _auditLogService.LogAsync(
            userId: request.AdminId,
            userEmail: request.AdminEmail,
            action: "UpdateTag",
            entityName: "Tag",
            entityId: tag.Id.ToString(),
            oldValues: oldValues,
            newValues: new { tag.Name, tag.Slug },
            ipAddress: request.IpAddress);

        return ApiResponse<object>.Ok(null!, "Cập nhật tag thành công");
    }
}