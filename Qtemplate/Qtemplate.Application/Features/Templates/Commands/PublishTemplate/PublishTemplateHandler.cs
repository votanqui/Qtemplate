using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Templates.Commands.PublishTemplate
{
    public class PublishTemplateHandler : IRequestHandler<PublishTemplateCommand, ApiResponse<object>>
    {
        private readonly ITemplateRepository _templateRepo;
        private readonly IAuditLogService _auditLogService;

        public PublishTemplateHandler(ITemplateRepository templateRepo, IAuditLogService auditLogService)
        {
            _templateRepo = templateRepo;
            _auditLogService = auditLogService;
        }

        public async Task<ApiResponse<object>> Handle(PublishTemplateCommand request, CancellationToken cancellationToken)
        {
            var template = await _templateRepo.GetByIdAsync(request.Id);
            if (template is null)
                return ApiResponse<object>.Fail("Không tìm thấy template");

            if (template.Status == "Published")
                return ApiResponse<object>.Fail("Template đã được publish rồi");

            var oldStatus = template.Status;
            template.Status = "Published";
            template.PublishedAt = DateTime.UtcNow;
            template.UpdatedAt = DateTime.UtcNow;

            await _templateRepo.UpdateAsync(template);

            await _auditLogService.LogAsync(
                userId: request.AdminId,
                userEmail: request.AdminEmail,
                action: "PublishTemplate",
                entityName: "Template",
                entityId: template.Id.ToString(),
                oldValues: new { Status = oldStatus },
                newValues: new { Status = "Published" },
                ipAddress: request.IpAddress
            );

            return ApiResponse<object>.Ok(null!, "Publish template thành công");
        }
    }
}
