using System;

namespace ProjectManagementSystem.WPF.Models
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Status { get; set; }
        public int Priority { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public int? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public int CommentsCount { get; set; }
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Status { get; set; }
        public int Priority { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string? AssigneeName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public int CommentsCount { get; set; }

        public string StatusText => Status switch
        {
            0 => "Новая",
            1 => "В работе",
            2 => "Завершена",
            3 => "Отменена",
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
            0 => "#2196F3",
            1 => "#FF9800", 
            2 => "#4CAF50", 
            3 => "#F44336", 
            _ => "#9E9E9E"
        };

        public string PriorityColor => Priority switch
        {
            0 => "#4CAF50", 
            1 => "#2196F3", 
            2 => "#FF9800", 
            3 => "#F44336", 
            _ => "#9E9E9E"  
        };

        public string CreatedAtText => CreatedAt.ToString("dd.MM.yyyy");
        public string UpdatedAtText => UpdatedAt.ToString("dd.MM.yyyy");
        public string PlannedHoursText => $"{PlannedHours} ч.";
        public string ActualHoursText => ActualHours.HasValue ? $"{ActualHours.Value:0.##} ч." : "Не указано";
        public string AssigneeDisplayName => AssigneeName ?? "Не назначен";
    }
}
