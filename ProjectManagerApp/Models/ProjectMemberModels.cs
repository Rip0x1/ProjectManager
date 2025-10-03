using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProjectManagerApp.Models
{
    public class ProjectMemberDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class ProjectMemberItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; }
        public DateTime JoinedAt { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        
        public string JoinedAtText => $"Присоединился: {JoinedAt:dd.MM.yyyy HH:mm}";
        
        public string RoleText => Role switch
        {
            0 => "Пользователь",
            1 => "Менеджер", 
            2 => "Администратор",
            _ => "Неизвестно"
        };

        public string RoleColor => Role switch
        {
            0 => "#757575",
            1 => "#2196F3",
            2 => "#F44336",
            _ => "#757575"
        };

        public string FormattedJoinedAt => JoinedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class AvailableUserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; }
    }

    public class AvailableUserItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int UserId => Id;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; }

        public string UserName => $"{FirstName} {LastName}";
        public string UserEmail => Email;
        public string UserRoleText => RoleText;
        
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
            0 => "#757575",
            1 => "#2196F3",
            2 => "#F44336",
            _ => "#757575"
        };

        public IRelayCommand? AddMemberCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class ProjectMembersResponse
    {
        public List<ProjectMemberDto> Members { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

    public class AvailableUsersResponse
    {
        public List<AvailableUserDto> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}
