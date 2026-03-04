using MediatR;
using Qtemplate.Application.DTOs.payments;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Queries.DownloadTemplate;

public class DownloadTemplateHandler : IRequestHandler<DownloadTemplateQuery, DownloadTemplateResult>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IUserDownloadRepository _downloadRepo;

    public DownloadTemplateHandler(
        ITemplateRepository templateRepo,
        IOrderRepository orderRepo,
        IUserDownloadRepository downloadRepo)
    {
        _templateRepo = templateRepo;
        _orderRepo = orderRepo;
        _downloadRepo = downloadRepo;
    }

    public async Task<DownloadTemplateResult> Handle(
        DownloadTemplateQuery request, CancellationToken cancellationToken)
    {
        var template = await _templateRepo.GetBySlugAsync(request.Slug);
        if (template is null) return Fail("Template không tồn tại");
        if (template.Status != "Published") return Fail("Template không khả dụng");
        if (string.IsNullOrEmpty(template.DownloadPath))
            return Fail("File download chưa được cấu hình");

        var orderId = Guid.Empty;
        if (!template.IsFree)
        {
            var order = await _orderRepo.GetPaidOrderByUserAndTemplateAsync(
                request.UserId, template.Id);
            if (order is null) return Fail("Bạn chưa mua template này");
            orderId = order.Id;
        }

        await _downloadRepo.UpsertAsync(
            request.UserId, template.Id, orderId,
            request.IpAddress, request.UserAgent);

        // External → redirect thẳng bằng DownloadPath
        if (template.StorageType != "Local")
            return new DownloadTemplateResult
            {
                Success = true,
                RedirectUrl = template.DownloadPath,
                FileName = $"{template.Slug}.zip"
            };

        // Local → stream file
        var filePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "private-storage",
            template.DownloadPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(filePath))
            return Fail($"File không tồn tại: {template.DownloadPath}");

        return new DownloadTemplateResult
        {
            Success = true,
            FilePath = filePath,
            FileName = $"{template.Slug}.zip",
            ContentType = "application/zip"
        };
    }

    private static DownloadTemplateResult Fail(string msg) =>
        new() { Success = false, ErrorMessage = msg };
}