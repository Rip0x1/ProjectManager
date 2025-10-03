using System.Threading.Tasks;

namespace ProjectManagementSystem.WPF.Services
{
    public interface IStatisticsService
    {
        Task<ProjectStatisticsDto> GetProjectStatisticsAsync(int projectId);
        Task<UserStatisticsDto> GetUserStatisticsAsync(int userId);
        Task<OverviewStatisticsDto> GetOverviewStatisticsAsync();
    }

    public class ProjectStatisticsDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public double CompletionRate { get; set; }
        public decimal TotalPlannedHours { get; set; }
        public decimal TotalActualHours { get; set; }
        public int ProjectMembers { get; set; }
        public int TotalComments { get; set; }
    }

    public class UserStatisticsDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int AssignedTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int CreatedTasks { get; set; }
        public double CompletionRate { get; set; }
    }

    public class OverviewStatisticsDto
    {
        public int TotalProjects { get; set; }
        public int TotalTasks { get; set; }
        public int TotalUsers { get; set; }
        public int TotalComments { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedTasks { get; set; }
        public int ActiveTasks { get; set; }
        public int TaskStatusNew { get; set; }
        public int TaskStatusInProgress { get; set; }
        public int TaskStatusReview { get; set; }
        public int TaskStatusCompleted { get; set; }
        public int TaskPriorityLow { get; set; }
        public int TaskPriorityMedium { get; set; }
        public int TaskPriorityHigh { get; set; }
        public int TaskPriorityCritical { get; set; }
        public int ProjectStatusPlanned { get; set; }
        public int ProjectStatusInProgress { get; set; }
        public int ProjectStatusCompleted { get; set; }
    }
}
