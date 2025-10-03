using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Database.Data;
using ProjectManagementSystem.Database.Entities;
using ProjectManagementSystem.API.Models.DTOs;

namespace ProjectManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProjects()
        {
            var projects = await _context.Projects
                .Include(p => p.Manager)
                .ToListAsync();

            var result = new List<object>();

            foreach (var project in projects)
            {
                var participantsCount = await _context.ProjectUsers
                    .Where(pu => pu.ProjectId == project.Id)
                    .CountAsync();

                var tasksCount = await _context.Tasks
                    .Where(t => t.ProjectId == project.Id)
                    .CountAsync();

                var commentsCount = await _context.Comments
                    .Where(c => c.TaskId.HasValue && _context.Tasks
                        .Where(t => t.ProjectId == project.Id)
                        .Select(t => t.Id)
                        .Contains(c.TaskId.Value))
                    .CountAsync();

                result.Add(new
                {
                    project.Id,
                    project.Name,
                    project.Description,
                    project.ManagerId,
                    project.Status,
                    project.CreatedAt,
                    project.Deadline,
                    ParticipantsCount = participantsCount,
                    TasksCount = tasksCount,
                    CommentsCount = commentsCount,
                    Manager = project.Manager == null ? null : new
                    {
                        project.Manager.Id,
                        project.Manager.FirstName,
                        project.Manager.LastName,
                        project.Manager.Email,
                        project.Manager.Role
                    }
                });
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Manager)
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.ManagerId,
                    p.Status,
                    p.CreatedAt,
                    p.Deadline,
                    Manager = p.Manager == null ? null : new
                    {
                        p.Manager.Id,
                        p.Manager.FirstName,
                        p.Manager.LastName,
                        p.Manager.Email,
                        p.Manager.Role
                    }
                })
                .FirstOrDefaultAsync();

            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }

        [HttpPost]
        public async Task<ActionResult<object>> PostProject(ProjectCreateUpdateDto dto)
        {
            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                ManagerId = dto.ManagerId,
                Status = dto.Status,
                Deadline = dto.Deadline,
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProject", new { id = project.Id }, new
            {
                project.Id,
                project.Name,
                project.Description,
                project.ManagerId,
                project.Status,
                project.CreatedAt,
                project.Deadline
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, ProjectCreateUpdateDto dto)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            project.Name = dto.Name;
            project.Description = dto.Description;
            project.ManagerId = dto.ManagerId;
            project.Status = dto.Status;
            project.Deadline = dto.Deadline;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserProjects(int userId)
        {
            try
            {
                var userProjects = await _context.ProjectUsers
                    .Where(pu => pu.UserId == userId)
                    .Include(pu => pu.Project)
                    .ThenInclude(p => p.Manager)
                    .ToListAsync();

                if (!userProjects.Any())
                {
                    return Ok(new List<object>());
                }

                var projectIds = userProjects.Select(pu => pu.Project.Id).ToList();

                var participantsStats = await _context.ProjectUsers
                    .Where(pu => projectIds.Contains(pu.ProjectId))
                    .GroupBy(pu => pu.ProjectId)
                    .Select(g => new { ProjectId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var tasksStats = await _context.Tasks
                    .Where(t => projectIds.Contains(t.ProjectId))
                    .GroupBy(t => t.ProjectId)
                    .Select(g => new { ProjectId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var completedTasksStats = await _context.Tasks
                    .Where(t => projectIds.Contains(t.ProjectId) && t.Status == 3)
                    .GroupBy(t => t.ProjectId)
                    .Select(g => new { ProjectId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var taskIds = await _context.Tasks
                    .Where(t => projectIds.Contains(t.ProjectId))
                    .Select(t => t.Id)
                    .ToListAsync();

                var commentsStats = await _context.Comments
                    .Where(c => c.TaskId.HasValue && taskIds.Contains(c.TaskId.Value))
                    .GroupBy(c => _context.Tasks.FirstOrDefault(t => t.Id == c.TaskId.Value).ProjectId)
                    .Select(g => new { ProjectId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var result = new List<object>();
                foreach (var pu in userProjects)
                {
                    var project = pu.Project;
                    var projectId = project.Id;
                    
                    var participantsCount = participantsStats.FirstOrDefault(p => p.ProjectId == projectId)?.Count ?? 0;
                    var tasksCount = tasksStats.FirstOrDefault(t => t.ProjectId == projectId)?.Count ?? 0;
                    var completedTasks = completedTasksStats.FirstOrDefault(t => t.ProjectId == projectId)?.Count ?? 0;
                    var commentsCount = commentsStats.FirstOrDefault(c => c.ProjectId == projectId)?.Count ?? 0;
                    
                    var progress = tasksCount > 0 ? (double)completedTasks / tasksCount * 100 : 0;

                    result.Add(new
                    {
                        Id = project.Id,
                        Name = project.Name,
                        Description = project.Description,
                        ManagerId = project.ManagerId,
                        CreatedAt = project.CreatedAt,
                        Deadline = project.Deadline,
                        Status = project.Status,
                        Manager = project.Manager != null ? new
                        {
                            Id = project.Manager.Id,
                            FirstName = project.Manager.FirstName,
                            LastName = project.Manager.LastName,
                            Email = project.Manager.Email,
                            Role = project.Manager.Role
                        } : null,
                        RoleInProject = pu.RoleInProject,
                        JoinedAt = pu.JoinedAt,
                        ParticipantsCount = participantsCount,
                        TasksCount = tasksCount,
                        CommentsCount = commentsCount,
                        Progress = Math.Round(progress, 1)
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка получения проектов пользователя: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}