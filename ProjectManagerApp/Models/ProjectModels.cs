using System;

namespace ProjectManagementSystem.WPF.Models
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ManagerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public UserDto Manager { get; set; }
    }

    public class ProjectItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ManagerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public string ManagerName { get; set; }
        
        // Дополнительные поля для модального окна
        public int Status { get; set; } = 0; // 0 - Активный, 1 - Завершен, 2 - Приостановлен
        public int ParticipantsCount { get; set; } = 0;
        public int TasksCount { get; set; } = 0;
        public int CommentsCount { get; set; } = 0;

        public string StatusText => Status switch
        {
            0 => "Активный",
            1 => "Завершен",
            2 => "Приостановлен",
            _ => "Неизвестно"
        };

        public string StatusColor => Status switch
        {
            0 => "#4CAF50", // Green - Active
            1 => "#2196F3", // Blue - Completed
            2 => "#FF9800", // Orange - Paused
            _ => "#9E9E9E"  // Grey - Unknown
        };

        public string CreatedAtText => CreatedAt.ToString("dd.MM.yyyy");
        public string DeadlineText => Deadline?.ToString("dd.MM.yyyy") ?? "Не указан";
    }
}


