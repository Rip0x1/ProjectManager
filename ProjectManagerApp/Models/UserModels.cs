using System.ComponentModel;

namespace ProjectManagementSystem.WPF.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public int Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ManagedProjectsCount { get; set; }
        public int AuthoredTasksCount { get; set; }
        public int AssignedTasksCount { get; set; }
        public int CommentsCount { get; set; }
    }

    public class CreateUserDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Role { get; set; }
    }

    public class UpdateUserDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string? Password { get; set; }
        public int Role { get; set; }
    }

    public class UserItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
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

        public string CreatedAtText => CreatedAt.ToLocalTime().ToString("dd.MM.yyyy");
        public string ActivityText => $"Проектов: {ManagedProjectsCount}, Задач: {AuthoredTasksCount + AssignedTasksCount}, Комментариев: {CommentsCount}";

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class UserRelatedProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Status { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public bool IsManager { get; set; }
        public bool IsMember { get; set; }
    }

    public class UserRelatedTaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Status { get; set; }
        public int Priority { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class UserRelatedCommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? TaskId { get; set; }
        public string? TaskTitle { get; set; }
        public string? ProjectName { get; set; }
    }

    public class UserRelatedItem
    {
        public int Id { get; set; }
        public int? RelatedId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Badge { get; set; } = string.Empty;
        public string BadgeColor { get; set; } = "#9E9E9E";
        public string Meta { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public string CardBackground { get; set; } = "#FFFFFF";
        public bool CanAdd { get; set; }
        public bool HasBadge => !string.IsNullOrWhiteSpace(Badge);
    }
}
