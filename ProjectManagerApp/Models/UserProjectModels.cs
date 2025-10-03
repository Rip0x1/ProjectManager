using System;
using System.ComponentModel;
using ProjectManagementSystem.WPF.Models;

namespace ProjectManagerApp.Models
{
    public class UserProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ManagerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public int Status { get; set; }
        public UserDto? Manager { get; set; }
        public string RoleInProject { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public int ParticipantsCount { get; set; }
        public int TasksCount { get; set; }
        public int CommentsCount { get; set; }
        public double Progress { get; set; }
    }

    public class UserProjectItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ManagerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public int Status { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public string RoleInProject { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        
        public int ParticipantsCount { get; set; } = 0;
        public int TasksCount { get; set; } = 0;
        public int CommentsCount { get; set; } = 0;
        public double Progress { get; set; } = 0.0;

        public string CreatedAtText => $"Создан: {CreatedAt.ToLocalTime():dd.MM.yyyy HH:mm}";
        public string DeadlineText => Deadline?.ToLocalTime().ToString("dd.MM.yyyy HH:mm") ?? "Не указан";
        public string JoinedAtText => $"Присоединился: {JoinedAt.ToLocalTime():dd.MM.yyyy HH:mm}";

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
            _ => "#757575"
        };

        public string RoleInProjectText => RoleInProject;

        public string RoleInProjectColor => RoleInProject switch
        {
            "Участник" => "#757575",
            "Менеджер" => "#2196F3",
            "Администратор" => "#F44336",
            _ => "#757575"
        };

        public string ParticipantsText => $"{ParticipantsCount} участников";
        public string TasksText => $"{TasksCount} задач";
        public string CommentsText => $"{CommentsCount} комментариев";
        public string ProgressText => $"{Progress:F0}%";

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
