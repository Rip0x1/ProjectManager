using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Database.Data;

namespace ProjectManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<object>> GetProjectStatistics(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }

            var totalTasks = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .CountAsync();

            var completedTasks = await _context.Tasks
                .Where(t => t.ProjectId == projectId && t.Status == 3)
                .CountAsync();

            var totalHours = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .SumAsync(t => t.PlannedHours);

            var actualHours = await _context.Tasks
                .Where(t => t.ProjectId == projectId && t.ActualHours.HasValue)
                .SumAsync(t => t.ActualHours.Value);

            var projectMembers = await _context.ProjectUsers
                .Where(pu => pu.ProjectId == projectId)
                .CountAsync();

            var totalComments = await _context.Comments
                .Where(c => c.Task.ProjectId == projectId)
                .CountAsync();

            return new
            {
                ProjectId = projectId,
                ProjectName = project.Name,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                CompletionRate = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0,
                TotalPlannedHours = totalHours,
                TotalActualHours = actualHours,
                ProjectMembers = projectMembers,
                TotalComments = totalComments
            };
        }

        [HttpGet("overview")]
        public async Task<ActionResult<object>> GetOverviewStatistics()
        {
            var totalProjects = await _context.Projects.CountAsync();
            var totalTasks = await _context.Tasks.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalComments = await _context.Comments.CountAsync();

            var activeProjects = await _context.Projects
                .Where(p => p.Status == 0)
                .CountAsync();

            var completedTasks = await _context.Tasks
                .Where(t => t.Status == 3)
                .CountAsync();

            var activeTasks = await _context.Tasks
                .Where(t => t.Status == 1)
                .CountAsync();

            var taskStatusNew = await _context.Tasks
                .Where(t => t.Status == 0)
                .CountAsync();

            var taskStatusInProgress = await _context.Tasks
                .Where(t => t.Status == 1)
                .CountAsync();

            var taskStatusReview = await _context.Tasks
                .Where(t => t.Status == 2)
                .CountAsync();

            var taskStatusCompleted = await _context.Tasks
                .Where(t => t.Status == 3)
                .CountAsync();

            var taskPriorityLow = await _context.Tasks
                .Where(t => t.Priority == 0)
                .CountAsync();

            var taskPriorityMedium = await _context.Tasks
                .Where(t => t.Priority == 1)
                .CountAsync();

            var taskPriorityHigh = await _context.Tasks
                .Where(t => t.Priority == 2)
                .CountAsync();

            var taskPriorityCritical = await _context.Tasks
                .Where(t => t.Priority == 3)
                .CountAsync();

            var projectStatusPlanned = await _context.Projects
                .Where(p => p.Status == 0)
                .CountAsync();

            var projectStatusInProgress = await _context.Projects
                .Where(p => p.Status == 0)
                .CountAsync();

            var projectStatusCompleted = await _context.Projects
                .Where(p => p.Status == 1)
                .CountAsync();

            return new
            {
                TotalProjects = totalProjects,
                TotalTasks = totalTasks,
                TotalUsers = totalUsers,
                TotalComments = totalComments,
                ActiveProjects = activeProjects,
                CompletedTasks = completedTasks,
                ActiveTasks = activeTasks,
                TaskStatusNew = taskStatusNew,
                TaskStatusInProgress = taskStatusInProgress,
                TaskStatusReview = taskStatusReview,
                TaskStatusCompleted = taskStatusCompleted,
                TaskPriorityLow = taskPriorityLow,
                TaskPriorityMedium = taskPriorityMedium,
                TaskPriorityHigh = taskPriorityHigh,
                TaskPriorityCritical = taskPriorityCritical,
                ProjectStatusPlanned = projectStatusPlanned,
                ProjectStatusInProgress = projectStatusInProgress,
                ProjectStatusCompleted = projectStatusCompleted
            };
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<object>> GetUserStatistics(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var assignedTasks = await _context.Tasks
                .Where(t => t.AssigneeId == userId)
                .CountAsync();

            var completedTasks = await _context.Tasks
                .Where(t => t.AssigneeId == userId && t.Status == 3)
                .CountAsync();

            var createdTasks = await _context.Tasks
                .Where(t => t.AuthorId == userId)
                .CountAsync();

            return new
            {
                UserId = userId,
                UserName = $"{user.FirstName} {user.LastName}",
                AssignedTasks = assignedTasks,
                CompletedTasks = completedTasks,
                CreatedTasks = createdTasks,
                CompletionRate = assignedTasks > 0 ? (double)completedTasks / assignedTasks * 100 : 0
            };
        }
    }
}