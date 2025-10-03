using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ProjectManagerApp.Models
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? TaskId { get; set; }
        public int AuthorId { get; set; }
        public CommentUserResponceDto CommentUserResponceDto { get; set; } = new();
        public CommentTaskReponseDto? CommentTaskReponseDto { get; set; }
    }

    public class CommentUserResponceDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
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

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}