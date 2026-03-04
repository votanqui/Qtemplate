using MediatR;
using Qtemplate.Application.DTOs.Template.Admin;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Templates.Queries.AdminGetTemplates
{
    public class AdminGetTemplatesHandler : IRequestHandler<AdminGetTemplatesQuery, ApiResponse<PaginatedResult<AdminTemplateListDto>>>
    {
        private readonly ITemplateRepository _templateRepo;

        public AdminGetTemplatesHandler(ITemplateRepository templateRepo) => _templateRepo = templateRepo;

        public async Task<ApiResponse<PaginatedResult<AdminTemplateListDto>>> Handle(AdminGetTemplatesQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _templateRepo.GetAdminListAsync(
                request.Search, request.Status, request.CategoryId, request.Page, request.PageSize);

            var dtos = items.Select(t => new AdminTemplateListDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                Status = t.Status,
                Price = t.Price,
                ThumbnailUrl = t.ThumbnailUrl,
                SalePrice = t.SalePrice,
                IsFree = t.IsFree,
                IsFeatured = t.IsFeatured,
                SalesCount = t.SalesCount,
                ViewCount = t.ViewCount,
                AverageRating = t.AverageRating,
                CategoryName = t.Category.Name,
                CreatedAt = t.CreatedAt,
                PublishedAt = t.PublishedAt
            }).ToList();

            return ApiResponse<PaginatedResult<AdminTemplateListDto>>.Ok(new PaginatedResult<AdminTemplateListDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }
}
