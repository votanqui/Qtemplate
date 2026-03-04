using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Commands.DeleteTemplateImage;

public class DeleteTemplateImageHandler : IRequestHandler<DeleteTemplateImageCommand, ApiResponse<object>>
{
    private readonly ITemplateImageRepository _imageRepo;
    private readonly IFileUploadService _fileUploadService;

    public DeleteTemplateImageHandler(ITemplateImageRepository imageRepo, IFileUploadService fileUploadService)
    {
        _imageRepo = imageRepo;
        _fileUploadService = fileUploadService;
    }

    public async Task<ApiResponse<object>> Handle(DeleteTemplateImageCommand request, CancellationToken cancellationToken)
    {
        var image = await _imageRepo.GetByIdAsync(request.ImageId);
        if (image is null)
            return ApiResponse<object>.Fail("Không tìm thấy ảnh");

        _fileUploadService.DeleteTemplateImage(image.ImageUrl);
        await _imageRepo.DeleteAsync(image);

        return ApiResponse<object>.Ok(null!, "Xóa ảnh thành công");
    }
}