using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ProjectManagementSystem.WPF.Models
{
    public class TaskAssigneeDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TaskAttachmentItem
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public int? UploadedById { get; set; }
        public string? UploadedByName { get; set; }
        public int? SourceCommentId { get; set; }
        public bool CanDelete { get; set; }
        public string FileSizeText => FileSize < 1024 * 1024
            ? $"{FileSize / 1024.0:0.#} КБ"
            : $"{FileSize / (1024.0 * 1024.0):0.##} МБ";
        public string UploadedAtText => UploadedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
    }

    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public int Priority { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public int? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public List<int> AssigneeIds { get; set; } = new();
        public List<TaskAssigneeDto> Assignees { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int CommentsCount { get; set; }
        public int AttachmentsCount { get; set; }
        public List<TaskAttachmentItem> Attachments { get; set; } = new();
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public int Priority { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public int? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public List<int> AssigneeIds { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int CommentsCount { get; set; }
        public int AttachmentsCount { get; set; }

        public string StatusText => Status switch
        {
            0 => "Новая",
            1 => "В работе",
            2 => "На проверке",
            3 => "Завершена",
            4 => "Отложено",
            5 => "Проблема",
            _ => "Неизвестно"
        };

        public string PriorityText => Priority switch
        {
            0 => "Низкий",
            1 => "Средний",
            2 => "Высокий",
            3 => "Критический",
            _ => "Неизвестно"
        };

        public string StatusColor => Status switch
        {
            0 => "#9E9E9E",
            1 => "#2196F3",
            2 => "#FF9800",
            3 => "#4CAF50",
            4 => "#607D8B",
            5 => "#F44336",
            _ => "#9E9E9E"
        };

        public string PriorityColor => Priority switch
        {
            0 => "#4CAF50",
            1 => "#FF9800",
            2 => "#FFC107",
            3 => "#F44336",
            _ => "#9E9E9E"
        };

        public string CreatedAtText => CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        public string UpdatedAtText => UpdatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        public string DueDateText => DueDate.HasValue ? DueDate.Value.ToLocalTime().ToString("dd.MM.yyyy") : "Не указан";
        public string CompletedAtText => CompletedAt.HasValue ? CompletedAt.Value.ToLocalTime().ToString("dd.MM.yyyy") : "Не указана";
        public string AssigneeDisplayName => !string.IsNullOrWhiteSpace(AssigneeName)
            ? AssigneeName
            : "Не назначено";

        public string DescriptionShort => TruncateText(Description, 80);

        public string DescriptionToolTip => string.IsNullOrWhiteSpace(Description) ? string.Empty : Description;

        private static string TruncateText(string? text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "—";
            }

            var normalized = text.Replace("\r\n", " ").Replace('\n', ' ').Trim();
            return normalized.Length <= maxLength
                ? normalized
                : normalized[..(maxLength - 3)] + "...";
        }

        public string AssigneeToolTip
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(AssigneeName))
                {
                    return AssigneeName.Replace(", ", "\n");
                }

                return AssigneeDisplayName == "Не назначено" ? string.Empty : AssigneeDisplayName;
            }
        }
    }

    public class CreateUpdateTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public int Priority { get; set; }
        public int? ProjectId { get; set; }
        public int AuthorId { get; set; }
        public int? AssigneeId { get; set; }
        public List<int> AssigneeIds { get; set; } = new();
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public partial class AssigneeChoice : ObservableObject
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;

        [ObservableProperty]
        private bool _isSelected;
    }

    public class PendingAttachment
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}
