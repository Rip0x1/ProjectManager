namespace ProjectManagementSystem.WPF.Models
{
    public enum UserRole
    {
        User = 0,      
        Manager = 1,   
        Admin = 2      
    }
    
    public static class UserRoleExtensions
    {
        public static string GetRoleName(this UserRole role)
        {
            return role switch
            {
                UserRole.User => "Пользователь",
                UserRole.Manager => "Менеджер",
                UserRole.Admin => "Администратор",
                _ => "Неизвестно"
            };
        }
        
        public static string GetRoleColor(this UserRole role)
        {
            return role switch
            {
                UserRole.User => "#4CAF50",      
                UserRole.Manager => "#2196F3",   
                UserRole.Admin => "#F44336",     
                _ => "#9E9E9E"                  
            };
        }
    }
}

