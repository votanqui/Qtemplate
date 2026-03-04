using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Commands.AddTemplateImage;

public class AddTemplateImageHandler : IRequestHandler<AddTemplateImageCommand, ApiResponse<object>>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly ITemplateImageRepository _imageRepo;
    private readonly IFileUploadService _fileUploadService;

    public AddTemplateImageHandler(
        ITemplateRepository templateRepo,
        ITemplateImageRepository imageRepo,
        IFileUploadService fileUploadService)
    {
        _templateRepo = templateRepo;
        _imageRepo = imageRepo;
        _fileUploadService = fileUploadService;
    }

    public async Task<ApiResponse<object>> Handle(AddTemplateImageCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateRepo.GetByIdAsync(request.TemplateId);
        if (template is null)
            return ApiResponse<object>.Fail("Không tìm thấy template");

        string url;
        try
        {
            await using var stream = request.File.OpenReadStream();
            url = await _fileUploadService.SaveTemplateImageAsync(stream, request.File.FileName, request.File.Length);
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse<object>.Fail(ex.Message);
        }

        await _imageRepo.AddAsync(new TemplateImage
        {
            TemplateId = request.TemplateId,
            ImageUrl = url,
            AltText = request.AltText,
            Type = request.Type,
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow
        });

        return ApiResponse<object>.Ok(null!, "Thêm ảnh thành công");
    }
}