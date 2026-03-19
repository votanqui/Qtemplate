using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Post;
using Qtemplate.Application.Features.Posts.Queries.GetPublishedPosts;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Posts.Commands.CreatePost
{
    public class CreatePostHandler : IRequestHandler<CreatePostCommand, ApiResponse<AdminPostDto>>
    {
        private readonly IPostRepository _repo;
        public CreatePostHandler(IPostRepository repo) => _repo = repo;

        public async Task<ApiResponse<AdminPostDto>> Handle(
            CreatePostCommand request, CancellationToken cancellationToken)
        {
            var d = request.Data;

            if (string.IsNullOrWhiteSpace(d.Title))
                return ApiResponse<AdminPostDto>.Fail("Tiêu đề không được để trống");

            if (string.IsNullOrWhiteSpace(d.Content))
                return ApiResponse<AdminPostDto>.Fail("Nội dung không được để trống");

            var validStatuses = new[] { "Draft", "Published", "Archived" };
            if (!validStatuses.Contains(d.Status))
                return ApiResponse<AdminPostDto>.Fail("Status không hợp lệ: Draft / Published / Archived");

            // Tạo slug từ title nếu không truyền vào
            var slug = string.IsNullOrWhiteSpace(d.Slug)
                ? GenerateSlug(d.Title)
                : d.Slug.Trim().ToLower();

            // Kiểm tra slug unique
            if (await _repo.SlugExistsAsync(slug))
                return ApiResponse<AdminPostDto>.Fail($"Slug '{slug}' đã tồn tại, hãy dùng slug khác");

            var post = new Post
            {
                Title = d.Title.Trim(),
                Slug = slug,
                Excerpt = d.Excerpt?.Trim(),
                Content = d.Content.Trim(),
                ThumbnailUrl = d.ThumbnailUrl?.Trim(),
                Status = d.Status,
                IsFeatured = d.IsFeatured,
                SortOrder = d.SortOrder,
                Tags = d.Tags,
                MetaTitle = d.MetaTitle?.Trim(),
                MetaDescription = d.MetaDescription?.Trim(),
                PublishedAt = d.Status == "Published"
                    ? (d.PublishedAt ?? DateTime.UtcNow)
                    : d.PublishedAt,
                AuthorId = request.AuthorId,
                AuthorName = request.AuthorName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(post);
            return ApiResponse<AdminPostDto>.Ok(
                GetPublishedPostsHandler.ToAdminDto(post), "Tạo bài viết thành công");
        }

        public static string GenerateSlug(string title)
        {
            // Chuẩn hóa tiếng Việt -> ASCII rồi slug hóa
            var normalized = title.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var c in normalized)
            {
                var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            var ascii = sb.ToString().Normalize(System.Text.NormalizationForm.FormC);

            // Thay thế ký tự đặc biệt tiếng Việt còn sót
            var map = new Dictionary<char, string>
            {
                ['đ'] = "d",
                ['Đ'] = "d",
                ['á'] = "a",
                ['à'] = "a",
                ['ả'] = "a",
                ['ã'] = "a",
                ['ạ'] = "a",
                ['ă'] = "a",
                ['ắ'] = "a",
                ['ặ'] = "a",
                ['ằ'] = "a",
                ['ẵ'] = "a",
                ['ẳ'] = "a",
                ['â'] = "a",
                ['ấ'] = "a",
                ['ậ'] = "a",
                ['ầ'] = "a",
                ['ẫ'] = "a",
                ['ẩ'] = "a",
                ['é'] = "e",
                ['è'] = "e",
                ['ẻ'] = "e",
                ['ẽ'] = "e",
                ['ẹ'] = "e",
                ['ê'] = "e",
                ['ế'] = "e",
                ['ệ'] = "e",
                ['ề'] = "e",
                ['ễ'] = "e",
                ['ể'] = "e",
                ['í'] = "i",
                ['ì'] = "i",
                ['ỉ'] = "i",
                ['ĩ'] = "i",
                ['ị'] = "i",
                ['ó'] = "o",
                ['ò'] = "o",
                ['ỏ'] = "o",
                ['õ'] = "o",
                ['ọ'] = "o",
                ['ô'] = "o",
                ['ố'] = "o",
                ['ộ'] = "o",
                ['ồ'] = "o",
                ['ổ'] = "o",
                ['ỗ'] = "o",
                ['ơ'] = "o",
                ['ớ'] = "o",
                ['ợ'] = "o",
                ['ờ'] = "o",
                ['ỡ'] = "o",
                ['ở'] = "o",
                ['ú'] = "u",
                ['ù'] = "u",
                ['ủ'] = "u",
                ['ũ'] = "u",
                ['ụ'] = "u",
                ['ư'] = "u",
                ['ứ'] = "u",
                ['ự'] = "u",
                ['ừ'] = "u",
                ['ữ'] = "u",
                ['ử'] = "u",
                ['ý'] = "y",
                ['ỳ'] = "y",
                ['ỷ'] = "y",
                ['ỹ'] = "y",
                ['ỵ'] = "y",
            };

            var result = new System.Text.StringBuilder();
            foreach (var c in ascii.ToLower())
            {
                if (map.TryGetValue(c, out var replacement))
                    result.Append(replacement);
                else if (char.IsLetterOrDigit(c))
                    result.Append(c);
                else if (c == ' ' || c == '-')
                    result.Append('-');
            }

            // Loại bỏ dấu gạch ngang trùng lặp
            var slug = System.Text.RegularExpressions.Regex.Replace(result.ToString(), @"-+", "-").Trim('-');
            return slug;
        }
    }
}
