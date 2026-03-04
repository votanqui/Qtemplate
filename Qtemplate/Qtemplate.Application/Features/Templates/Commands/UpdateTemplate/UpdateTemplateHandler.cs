using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Templates.Commands.UpdateTemplate
{
    public class UpdateTemplateHandler : IRequestHandler<UpdateTemplateCommand, ApiResponse<object>>
    {
        private readonly ITemplateRepository _templateRepo;
        private readonly IAuditLogService _auditLogService;

        public UpdateTemplateHandler(ITemplateRepository templateRepo, IAuditLogService auditLogService)
        {
            _templateRepo = templateRepo;
            _auditLogService = auditLogService;
        }

        public async Task<ApiResponse<object>> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
        {
            var template = await _templateRepo.GetByIdWithDetailsAsync(request.Id);
            if (template is null)
                return ApiResponse<object>.Fail("Không tìm thấy template");

            var dto = request.Dto;

            // Kiểm tra slug trùng (trừ chính nó)
            if (template.Slug != dto.Slug && await _templateRepo.SlugExistsAsync(dto.Slug))
                return ApiResponse<object>.Fail("Slug này đã tồn tại");

            var oldValues = new { template.Name, template.Slug, template.Price, template.Status };

            template.CategoryId = dto.CategoryId;
            template.Name = dto.Name.Trim();
            template.Slug = dto.Slug.Trim().ToLower();
            template.ShortDescription = dto.ShortDescription?.Trim();
            template.Description = dto.Description?.Trim();
            template.ChangeLog = dto.ChangeLog?.Trim();
            template.Price = dto.Price;
            template.SalePrice = dto.SalePrice;
            template.SaleStartAt = dto.SaleStartAt;
            template.SaleEndAt = dto.SaleEndAt;
            template.ThumbnailUrl = dto.ThumbnailUrl;
            template.PreviewUrl = dto.PreviewUrl;
            template.TechStack = dto.TechStack;
            template.CompatibleWith = dto.CompatibleWith;
            template.FileFormat = dto.FileFormat;
            template.Version = dto.Version;
            template.IsFeatured = dto.IsFeatured;
            template.IsFree = dto.IsFree;
            template.UpdatedAt = DateTime.UtcNow;

            // Cập nhật Tags — xóa cũ thêm mới
            template.TemplateTags = dto.TagIds
                .Select(tagId => new TemplateTag { TemplateId = template.Id, TagId = tagId })
                .ToList();

            // Cập nhật Features — xóa cũ thêm mới
            template.Features = dto.Features
                .Select((f, i) => new TemplateFeature { TemplateId = template.Id, Feature = f, SortOrder = i })
                .ToList();

            await _templateRepo.UpdateAsync(template);

            await _auditLogService.LogAsync(
                userId: request.AdminId,
                userEmail: request.AdminEmail,
                action: "UpdateTemplate",
                entityName: "Template",
                entityId: template.Id.ToString(),
                oldValues: oldValues,
                newValues: new { template.Name, template.Slug, template.Price, template.Status },
                ipAddress: request.IpAddress
            );

            return ApiResponse<object>.Ok(null!, "Cập nhật template thành công");
        }
    }
}
