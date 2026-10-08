using ProjectManagerApp.Models;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using System.Collections.Generic;
using System.Linq;

namespace ProjectManagementSystem.WPF.Helpers
{
    public static class CommentPermissionHelper
    {
        public static void ApplyPermissions(
            CommentItem comment,
            IPermissionService permissionService,
            int projectManagerId)
        {
            comment.CanDelete = permissionService.CanDeleteComment(comment.AuthorId, projectManagerId);
            comment.CanAddAttachment = permissionService.CanAddCommentAttachment(comment.AuthorId);

            foreach (var attachment in comment.Attachments)
            {
                attachment.CanDelete = permissionService.CanDeleteCommentAttachment(
                    comment.AuthorId,
                    attachment.UploadedById,
                    projectManagerId);
            }
        }

        public static CommentItem MapCommentDto(
            CommentDto dto,
            IPermissionService permissionService,
            int projectManagerId)
        {
            var authorName = $"{dto.CommentUserResponceDto.FirstName} {dto.CommentUserResponceDto.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(authorName))
            {
                authorName = dto.CommentUserResponceDto.Login;
            }

            var comment = new CommentItem
            {
                Id = dto.Id,
                Content = dto.Content,
                CreatedAt = dto.CreatedAt,
                TaskId = dto.TaskId,
                AuthorId = dto.AuthorId,
                AuthorName = authorName,
                Attachments = new System.Collections.ObjectModel.ObservableCollection<TaskAttachmentItem>(
                    (dto.Attachments ?? new List<CommentAttachmentDto>()).Select(a => MapAttachment(a, dto.Id)))
            };

            ApplyPermissions(comment, permissionService, projectManagerId);
            return comment;
        }

        private static TaskAttachmentItem MapAttachment(CommentAttachmentDto item, int commentId)
        {
            return new TaskAttachmentItem
            {
                Id = item.Id,
                FileName = item.FileName,
                ContentType = item.ContentType,
                FileSize = item.FileSize,
                UploadedAt = item.UploadedAt,
                UploadedById = item.UploadedById,
                SourceCommentId = commentId
            };
        }
    }
}
