using System;

namespace ProjectManagementSystem.WPF.Models
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ManagerId { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public UserDto Manager { get; set; }
    }

    public class CreateUpdateProjectDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int ManagerId { get; set; }
        public int Status { get; set; }
        public DateTime? Deadline { get; set; }
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
        
        public int Status { get; set; } = 0;
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
            0 => "#4CAF50", 
            1 => "#2196F3", 
            2 => "#FF9800",
            _ => "#9E9E9E" 
        };

        public string CreatedAtText => CreatedAt.ToString("dd.MM.yyyy");
        public string DeadlineText => Deadline?.ToString("dd.MM.yyyy") ?? "Не указан";
    }
}


