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

        // GET: api/Statistics/project/5
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

            return new
            {
                ProjectId = projectId,
                ProjectName = project.Name,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                CompletionRate = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0,
                TotalPlannedHours = totalHours,
                TotalActualHours = actualHours
            };
        }

        // GET: api/Statistics/user/5
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