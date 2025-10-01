using System.ComponentModel;

namespace ProjectManagementSystem.WPF.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ManagedProjectsCount { get; set; }
        public int AuthoredTasksCount { get; set; }
        public int AssignedTasksCount { get; set; }
        public int CommentsCount { get; set; }
    }

    public class UserItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ManagedProjectsCount { get; set; }
        public int AuthoredTasksCount { get; set; }
        public int AssignedTasksCount { get; set; }
        public int CommentsCount { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        public string RoleText => Role switch
        {
            0 => "Пользователь",
            1 => "Менеджер", 
            2 => "Администратор",
            _ => "Неизвестно"
        };

        public string RoleColor => Role switch
        {
            0 => "#2196F3", 
            1 => "#FF9800", 
            2 => "#F44336", 
            _ => "#9E9E9E" 
        };

        public string CreatedAtText => CreatedAt.ToString("dd.MM.yyyy");
        public string ActivityText => $"Проектов: {ManagedProjectsCount}, Задач: {AuthoredTasksCount + AssignedTasksCount}, Комментариев: {CommentsCount}";

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
