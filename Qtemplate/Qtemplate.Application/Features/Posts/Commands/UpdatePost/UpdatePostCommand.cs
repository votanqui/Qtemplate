using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Posts.Commands.UpdatePost
{
    public class UpdatePostCommand : IRequest<ApiResponse<AdminPostDto>>
    {
        public int Id { get; set; }
        public UpsertPostDto Data { get; set; } = null!;
    }
}
