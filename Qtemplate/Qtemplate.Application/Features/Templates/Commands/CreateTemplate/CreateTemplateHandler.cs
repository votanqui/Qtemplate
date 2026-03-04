using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Commands.CreateTemplate;

public class CreateTemplateHandler : IRequestHandler<CreateTemplateCommand, ApiResponse<Guid>>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly IAuditLogService _auditLogService;

    public CreateTemplateHandler(ITemplateRepository templateRepo, IAuditLogService auditLogService)
    {
        _templateRepo = templateRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // IsFree thì giá phải = 0
        if (dto.IsFree && dto.Price != 0)
            return ApiResponse<Guid>.Fail("Template miễn phí phải có giá = 0");

        // Có giá thì không được free
        if (!dto.IsFree && dto.Price <= 0)
            return ApiResponse<Guid>.Fail("Template trả phí phải có giá > 0");

        if (await _templateRepo.SlugExistsAsync(dto.Slug))
            return ApiResponse<Guid>.Fail("Slug này đã tồn tại");

        if (!await _templateRepo.CategoryExistsAsync(dto.CategoryId))
            return ApiResponse<Guid>.Fail("Category không tồn tại");

        if (dto.TagIds.Any() && !await _templateRepo.AllTagsExistAsync(dto.TagIds))
            return ApiResponse<Guid>.Fail("Một hoặc nhiều Tag không tồn tại");

        var templateId = Guid.NewGuid();

        var template = new Template
        {
            Id = templateId,
            CategoryId = dto.CategoryId,
            Name = dto.Name.Trim(),
            Slug = dto.Slug.Trim().ToLower(),
            ShortDescription = dto.ShortDescription?.Trim(),
            Description = dto.Description?.Trim(),
            Price = dto.IsFree ? 0 : dto.Price,  // ← force 0 nếu free
            SalePrice = null,   // sale có API riêng
            SaleStartAt = null,
            SaleEndAt = null,
            DownloadPath = null,   // upload có API riêng
            ThumbnailUrl = null,
            PreviewFolder = null,
            PreviewType = "None",
            TechStack = dto.TechStack,
            CompatibleWith = dto.CompatibleWith,
            FileFormat = dto.FileFormat,
            Version = dto.Version ?? "1.0.0",
            IsFeatured = dto.IsFeatured,
            IsFree = dto.IsFree,
            IsNew = true,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            TemplateTags = dto.TagIds.Select(tagId => new TemplateTag
            {
                TemplateId = templateId,
                TagId = tagId
            }).ToList(),
            Features = dto.Features.Select((f, i) => new TemplateFeature
            {
                TemplateId = templateId,
                Feature = f,
                SortOrder = i
            }).ToList()
        };

        await _templateRepo.AddAsync(template);

        await _auditLogService.LogAsync(
            userId: request.AdminId, userEmail: request.AdminEmail,
            action: "CreateTemplate", entityName: "Template",
            entityId: template.Id.ToString(),
            newValues: new { template.Name, template.Slug, template.Price, template.IsFree },
            ipAddress: request.IpAddress);

        return ApiResponse<Guid>.Ok(template.Id, "Tạo template thành công");
    }
}
