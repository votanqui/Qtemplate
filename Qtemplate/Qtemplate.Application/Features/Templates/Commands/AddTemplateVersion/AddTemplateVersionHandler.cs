using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Commands.AddTemplateVersion;

public class AddTemplateVersionHandler : IRequestHandler<AddTemplateVersionCommand, ApiResponse<int>>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly ITemplateVersionRepository _versionRepo;
    private readonly IFileUploadService _fileUploadService;
    private readonly IAuditLogService _auditLogService;
    private readonly IMediaFileRepository _mediaRepo;   // ← thêm

    public AddTemplateVersionHandler(
        ITemplateRepository templateRepo,
        ITemplateVersionRepository versionRepo,
        IFileUploadService fileUploadService,
        IAuditLogService auditLogService,
        IMediaFileRepository mediaRepo)             // ← thêm
    {
        _templateRepo = templateRepo;
        _versionRepo = versionRepo;
        _fileUploadService = fileUploadService;
        _auditLogService = auditLogService;
        _mediaRepo = mediaRepo;
    }

    public async Task<ApiResponse<int>> Handle(
        AddTemplateVersionCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateRepo.GetByIdAsync(request.TemplateId);
        if (template is null)
            return ApiResponse<int>.Fail("Không tìm thấy template");

        var existingVersions = await _versionRepo.GetByTemplateIdAsync(request.TemplateId);

        if (existingVersions.Any(v => v.Version == request.Version.Trim()))
            return ApiResponse<int>.Fail($"Version {request.Version} đã tồn tại");

        // ── Xác định downloadPath theo StorageType ──
        string downloadPath;
        string storageType;

        if (request.StorageType is "GoogleDrive" or "S3" or "R2")
        {
            if (string.IsNullOrEmpty(request.ExternalUrl))
                return ApiResponse<int>.Fail("ExternalUrl không được để trống");

            if (!Uri.TryCreate(request.ExternalUrl, UriKind.Absolute, out _))
                return ApiResponse<int>.Fail("ExternalUrl không hợp lệ");

            downloadPath = request.ExternalUrl;
            storageType = request.StorageType;
        }
        else
        {
            // Local — giữ nguyên logic cũ
            if (request.File is null)
                return ApiResponse<int>.Fail("File không được để trống");

            try
            {
                await using var stream = request.File.OpenReadStream();
                downloadPath = await _fileUploadService.SaveVersionZipAsync(
                    stream, request.File.FileName, request.File.Length,
                    request.TemplateId, request.Version);
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<int>.Fail(ex.Message);
            }

            storageType = "Local";
        }

        // Nếu đây là version đầu tiên → xóa file tạm cũ
        if (!existingVersions.Any())
            _fileUploadService.DeleteDownloadFile(request.TemplateId);

        // Bỏ IsLatest của version cũ
        foreach (var v in existingVersions.Where(v => v.IsLatest))
        {
            v.IsLatest = false;
            await _versionRepo.UpdateAsync(v);
        }

        var newVersion = new TemplateVersion
        {
            TemplateId = request.TemplateId,
            Version = request.Version.Trim(),
            ChangeLog = request.ChangeLog?.Trim(),
            DownloadPath = downloadPath,
            IsLatest = true,
            CreatedAt = DateTime.UtcNow
        };
        await _versionRepo.AddAsync(newVersion);

        // Cập nhật template — CHỈ DownloadPath, không đụng Preview
        template.Version = request.Version.Trim();
        template.DownloadPath = downloadPath;
        template.StorageType = storageType;   // ← thêm
        template.ChangeLog = request.ChangeLog?.Trim();
        template.UpdatedAt = DateTime.UtcNow;
        await _templateRepo.UpdateAsync(template);

        // Log vào MediaFile
        await _mediaRepo.AddAsync(new MediaFile
        {
            FileName = Path.GetFileName(downloadPath.Split('?')[0]),
            OriginalName = request.File?.FileName ?? request.ExternalUrl ?? "",
            Url = downloadPath,
            StorageType = storageType,
            FileSize = request.File?.Length ?? 0,
            TemplateId = request.TemplateId,
            UploadedBy = request.AdminId,
            CreatedAt = DateTime.UtcNow
        });

        await _auditLogService.LogAsync(
            userId: request.AdminId,
            userEmail: request.AdminEmail,
            action: "AddTemplateVersion",
            entityName: "TemplateVersion",
            entityId: newVersion.Id.ToString(),
            newValues: new { request.Version, downloadPath, storageType },
            ipAddress: request.IpAddress);

        return ApiResponse<int>.Ok(newVersion.Id, $"Đã thêm version {request.Version}");
    }
}