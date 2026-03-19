using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Post;
using Qtemplate.Application.Features.Posts.Commands.CreatePost;
using Qtemplate.Application.Features.Posts.Queries.GetPublishedPosts;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Posts.Commands.UpdatePost
{
    public class UpdatePostHandler : IRequestHandler<UpdatePostCommand, ApiResponse<AdminPostDto>>
    {
        private readonly IPostRepository _repo;
        public UpdatePostHandler(IPostRepository repo) => _repo = repo;

        public async Task<ApiResponse<AdminPostDto>> Handle(
            UpdatePostCommand request, CancellationToken cancellationToken)
        {
            var post = await _repo.GetByIdAsync(request.Id);
            if (post == null)
                return ApiResponse<AdminPostDto>.Fail("Bài viết không tồn tại");

            var d = request.Data;

            if (string.IsNullOrWhiteSpace(d.Title))
                return ApiResponse<AdminPostDto>.Fail("Tiêu đề không được để trống");

            var validStatuses = new[] { "Draft", "Published", "Archived" };
            if (!validStatuses.Contains(d.Status))
                return ApiResponse<AdminPostDto>.Fail("Status không hợp lệ");

            // Xử lý slug
            var slug = string.IsNullOrWhiteSpace(d.Slug)
                ? CreatePostHandler.GenerateSlug(d.Title)
                : d.Slug.Trim().ToLower();

            if (await _repo.SlugExistsAsync(slug, post.Id))
                return ApiResponse<AdminPostDto>.Fail($"Slug '{slug}' đã được dùng bởi bài viết khác");

            // Cập nhật PublishedAt khi chuyển sang Published lần đầu
            if (d.Status == "Published" && post.PublishedAt == null)
                post.PublishedAt = d.PublishedAt ?? DateTime.UtcNow;
            else if (d.PublishedAt.HasValue)
                post.PublishedAt = d.PublishedAt;

            post.Title = d.Title.Trim();
            post.Slug = slug;
            post.Excerpt = d.Excerpt?.Trim();
            post.Content = d.Content?.Trim() ?? post.Content;
            post.ThumbnailUrl = d.ThumbnailUrl?.Trim();
            post.Status = d.Status;
            post.IsFeatured = d.IsFeatured;
            post.SortOrder = d.SortOrder;
            post.Tags = d.Tags;
            post.MetaTitle = d.MetaTitle?.Trim();
            post.MetaDescription = d.MetaDescription?.Trim();
            post.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(post);
            return ApiResponse<AdminPostDto>.Ok(
                GetPublishedPostsHandler.ToAdminDto(post), "Cập nhật bài viết thành công");
        }
    }
}
