namespace ProjectManagementSystem.WPF.Models
{
    public class PermissionMatrixRow
    {
        public string Category { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserAccess { get; set; } = string.Empty;
        public string ManagerAccess { get; set; } = string.Empty;
        public string AdminAccess { get; set; } = string.Empty;
    }
}
