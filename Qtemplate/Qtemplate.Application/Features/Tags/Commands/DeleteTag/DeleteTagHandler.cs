using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Tags.Commands.DeleteTag;

public class DeleteTagHandler : IRequestHandler<DeleteTagCommand, ApiResponse<object>>
{
    private readonly ITagRepository _tagRepo;
    private readonly IAuditLogService _auditLogService;

    public DeleteTagHandler(ITagRepository tagRepo, IAuditLogService auditLogService)
    {
        _tagRepo = tagRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<object>> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _tagRepo.GetByIdAsync(request.Id);
        if (tag is null)
            return ApiResponse<object>.Fail("Không tìm thấy tag");

        await _tagRepo.DeleteAsync(tag);

        await _auditLogService.LogAsync(
            userId: request.AdminId,
            userEmail: request.AdminEmail,
            action: "DeleteTag",
            entityName: "Tag",
            entityId: request.Id.ToString(),
            oldValues: new { tag.Name, tag.Slug },
            ipAddress: request.IpAddress);

        return ApiResponse<object>.Ok(null!, "Xóa tag thành công");
    }
}