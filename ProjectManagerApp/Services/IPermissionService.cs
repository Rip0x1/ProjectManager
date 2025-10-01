using ProjectManagementSystem.WPF.Models;

namespace ProjectManagementSystem.WPF.Services
{
    public interface IPermissionService
    {
        bool CanCreateProject();
        
        bool CanEditProject(int projectManagerId);
        
        bool CanDeleteProject(int projectManagerId);
        
        bool CanCreateTask();
        
        bool CanEditTask(int taskAuthorId);
        
        bool CanDeleteTask(int taskAuthorId);
        
        bool CanManageUsers();
        
        bool CanAssignTasks();
        
        bool IsAdmin();
        
        bool IsManagerOrAbove();
        
        UserRole GetCurrentUserRole();
    }
    
    public class PermissionService : IPermissionService
    {
        private readonly IAuthService _authService;
        
        public PermissionService(IAuthService authService)
        {
            _authService = authService;
        }
        
        public UserRole GetCurrentUserRole()
        {
            return (UserRole)_authService.CurrentUserRole;
        }
        
        public bool IsAdmin()
        {
            return GetCurrentUserRole() == UserRole.Admin;
        }
        
        public bool IsManagerOrAbove()
        {
            var role = GetCurrentUserRole();
            return role == UserRole.Manager || role == UserRole.Admin;
        }
        
        public bool CanCreateProject()
        {
            return IsManagerOrAbove();
        }
        
        public bool CanEditProject(int projectManagerId)
        {
            if (IsAdmin()) return true;
            
            if (GetCurrentUserRole() == UserRole.Manager)
            {
                return _authService.CurrentUserId == projectManagerId;
            }
            
            return false;
        }
        
        public bool CanDeleteProject(int projectManagerId)
        {
            if (IsAdmin()) return true;
            
            if (GetCurrentUserRole() == UserRole.Manager)
            {
                return _authService.CurrentUserId == projectManagerId;
            }
            
            return false;
        }
        
        public bool CanCreateTask()
        {
            return IsManagerOrAbove();
        }
        
        public bool CanEditTask(int taskAuthorId)
        {
            if (IsAdmin()) return true;
            
            if (GetCurrentUserRole() == UserRole.Manager)
            {
                return _authService.CurrentUserId == taskAuthorId;
            }
            
            return false;
        }
        
        public bool CanDeleteTask(int taskAuthorId)
        {
            if (IsAdmin()) return true;
            
            if (GetCurrentUserRole() == UserRole.Manager)
            {
                return _authService.CurrentUserId == taskAuthorId;
            }
            
            return false;
        }
        
        public bool CanManageUsers()
        {
            return IsAdmin();
        }
        
        public bool CanAssignTasks()
        {
            return IsManagerOrAbove();
        }
    }
}

