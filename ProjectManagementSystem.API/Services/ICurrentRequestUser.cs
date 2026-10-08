namespace ProjectManagementSystem.API.Services
{
    public interface ICurrentRequestUser
    {
        int? UserId { get; set; }
        string Login { get; set; }
        string FullName { get; set; }
        int Role { get; set; }
        bool SuppressAudit { get; set; }
        bool IsAdmin { get; }
    }

    public class CurrentRequestUser : ICurrentRequestUser
    {
        public int? UserId { get; set; }
        public string Login { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int Role { get; set; }
        public bool SuppressAudit { get; set; }
        public bool IsAdmin => Role == 2;
    }
}
