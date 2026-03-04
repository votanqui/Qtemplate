using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Commands.DeleteTemplateVersion;

public class DeleteTemplateVersionHandler : IRequestHandler<DeleteTemplateVersionCommand, ApiResponse<object>>
{
    private readonly ITemplateVersionRepository _versionRepo;
    private readonly ITemplateRepository _templateRepo;
    private readonly IFileUploadService _fileUploadService;
    private readonly IAuditLogService _auditLogService;

    public DeleteTemplateVersionHandler(
        ITemplateVersionRepository versionRepo,
        ITemplateRepository templateRepo,
        IFileUploadService fileUploadService,
        IAuditLogService auditLogService)
    {
        _versionRepo = versionRepo;
        _templateRepo = templateRepo;
        _fileUploadService = fileUploadService;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<object>> Handle(DeleteTemplateVersionCommand request, CancellationToken cancellationToken)
    {
        var allVersions = await _versionRepo.GetByTemplateIdAsync(request.TemplateId);

        var version = allVersions.FirstOrDefault(v => v.Version == request.Version);
        if (version is null)
            return ApiResponse<object>.Fail($"Không tìm thấy version {request.Version}");

        if (allVersions.Count == 1)
            return ApiResponse<object>.Fail("Không thể xóa version duy nhất");

        _fileUploadService.DeleteVersionZip(version.TemplateId, version.Version);

        var wasLatest = version.IsLatest;
        await _versionRepo.DeleteAsync(version);

        if (wasLatest)
        {
            var newLatest = allVersions
                .Where(v => v.Id != version.Id)
                .OrderByDescending(v => v.CreatedAt)
                .First();

            newLatest.IsLatest = true;
            await _versionRepo.UpdateAsync(newLatest);

            var template = await _templateRepo.GetByIdAsync(request.TemplateId);
            if (template is not null)
            {
                template.Version = newLatest.Version;
                template.DownloadPath = newLatest.DownloadPath;
                template.UpdatedAt = DateTime.UtcNow;
                await _templateRepo.UpdateAsync(template);
            }
        }

        await _auditLogService.LogAsync(
            userId: request.AdminId, userEmail: request.AdminEmail,
            action: "DeleteTemplateVersion", entityName: "TemplateVersion",
            entityId: version.Id.ToString(),
            oldValues: new { version.Version, version.DownloadPath },
            ipAddress: request.IpAddress);

        return ApiResponse<object>.Ok(null!, $"Đã xóa version {request.Version}");
    }

}