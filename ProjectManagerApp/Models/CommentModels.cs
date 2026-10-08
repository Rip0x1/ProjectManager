using System.Collections.ObjectModel;
using System.ComponentModel;
using ProjectManagementSystem.WPF.Models;

namespace ProjectManagerApp.Models
{
    public class CommentAttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public int? UploadedById { get; set; }
    }
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? TaskId { get; set; }
        public int AuthorId { get; set; }
        public CommentUserResponceDto CommentUserResponceDto { get; set; } = new();
        public CommentTaskReponseDto? CommentTaskReponseDto { get; set; }
        public List<CommentAttachmentDto> Attachments { get; set; } = new();
    }

    public class CommentUserResponceDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
    }

    public class CommentTaskReponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CreateCommentDto
    {
        public string Content { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public int? TaskId { get; set; }
    }

    public class CommentsResponse
    {
        public List<CommentDto> Comments { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

    public class CommentItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? TaskId { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string FormattedCreatedAt => CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        public bool CanDelete { get; set; }
        public bool CanAddAttachment { get; set; }
        public ObservableCollection<TaskAttachmentItem> Attachments { get; set; } = new();
        public bool HasAttachments => Attachments.Count > 0;

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}