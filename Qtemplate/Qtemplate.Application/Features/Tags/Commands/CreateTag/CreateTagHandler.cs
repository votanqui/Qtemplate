using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Tags.Commands.CreateTag;

public class CreateTagHandler : IRequestHandler<CreateTagCommand, ApiResponse<int>>
{
    private readonly ITagRepository _tagRepo;
    private readonly IAuditLogService _auditLogService;

    public CreateTagHandler(ITagRepository tagRepo, IAuditLogService auditLogService)
    {
        _tagRepo = tagRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<int>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        if (await _tagRepo.SlugExistsAsync(request.Dto.Slug))
            return ApiResponse<int>.Fail("Slug đã tồn tại");

        var tag = new Tag
        {
            Name = request.Dto.Name.Trim(),
            Slug = request.Dto.Slug.Trim().ToLower()
        };

        await _tagRepo.AddAsync(tag);

        await _auditLogService.LogAsync(
            userId: request.AdminId,
            userEmail: request.AdminEmail,
            action: "CreateTag",
            entityName: "Tag",
            entityId: tag.Id.ToString(),
            newValues: new { tag.Name, tag.Slug },
            ipAddress: request.IpAddress);

        return ApiResponse<int>.Ok(tag.Id, "Tạo tag thành công");
    }
}