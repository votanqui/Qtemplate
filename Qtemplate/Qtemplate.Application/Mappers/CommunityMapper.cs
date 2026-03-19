using Qtemplate.Application.DTOs.Community;
using Qtemplate.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Mappers
{
    public static class CommunityMapper
    {
        public static CommunityPostDto ToPostDto(
            CommunityPost p,
            Guid? currentUserId = null,
            IEnumerable<int>? likedIds = null) => new()
            {
                Id = p.Id,
                UserId = p.UserId,
                AuthorName = p.User?.FullName ?? "Unknown",
                AuthorAvatar = p.User?.AvatarUrl,
                Content = p.Content,
                ImageUrl = p.ImageUrl,
                LikeCount = p.LikeCount,
                CommentCount = p.CommentCount,
                IsLikedByMe = likedIds?.Contains(p.Id) ?? false,
                IsOwner = currentUserId.HasValue && p.UserId == currentUserId.Value,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
            };

        public static AdminCommunityPostDto ToAdminPostDto(CommunityPost p) => new()
        {
            Id = p.Id,
            UserId = p.UserId,
            AuthorName = p.User?.FullName ?? "Unknown",
            AuthorEmail = p.User?.Email ?? string.Empty,
            AuthorAvatar = p.User?.AvatarUrl,
            Content = p.Content,
            ImageUrl = p.ImageUrl,
            LikeCount = p.LikeCount,
            CommentCount = p.CommentCount,
            IsHidden = p.IsHidden,
            HideReason = p.HideReason,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
        };

        public static CommunityCommentDto ToCommentDto(
            CommunityComment c,
            Guid? currentUserId = null) => new()
            {
                Id = c.Id,
                PostId = c.PostId,
                ParentId = c.ParentId,
                UserId = c.UserId,
                AuthorName = c.User?.FullName ?? "Unknown",
                AuthorAvatar = c.User?.AvatarUrl,
                Content = c.Content,
                IsOwner = currentUserId.HasValue && c.UserId == currentUserId.Value,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Replies = c.Replies
                .Where(r => !r.IsHidden)
                .OrderBy(r => r.CreatedAt)
                .Select(r => ToCommentDto(r, currentUserId))
                .ToList(),
            };

        public static AdminCommunityCommentDto ToAdminCommentDto(CommunityComment c) => new()
        {
            Id = c.Id,
            PostId = c.PostId,
            ParentId = c.ParentId,
            UserId = c.UserId,
            AuthorName = c.User?.FullName ?? "Unknown",
            AuthorAvatar = c.User?.AvatarUrl,
            Content = c.Content,
            IsHidden = c.IsHidden,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
        };
    }
}
