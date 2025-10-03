using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.API.Models.DTOs;
using ProjectManagementSystem.Database.Data;
using ProjectManagementSystem.Database.Entities;
using Task = ProjectManagementSystem.Database.Entities.Task;

namespace ProjectManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetTasks(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null,
            [FromQuery] int? projectId = null)
        {
            try
            {
                var query = _context.Tasks.AsNoTracking();

                if (projectId.HasValue)
                {
                    query = query.Where(t => t.ProjectId == projectId.Value);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(t => t.Title.Contains(search) || t.Description.Contains(search));
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var tasks = await query
                    .OrderByDescending(t => t.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new TaskResponseDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        Status = t.Status,
                        Priority = t.Priority,
                        ProjectId = t.ProjectId,
                        ProjectName = t.Project != null ? t.Project.Name : "Неизвестный проект",
                        AuthorId = t.AuthorId,
                        AuthorName = t.Author != null ? 
                            (!string.IsNullOrEmpty(t.Author.FirstName) || !string.IsNullOrEmpty(t.Author.LastName) 
                                ? $"{t.Author.FirstName} {t.Author.LastName}".Trim() 
                                : t.Author.Email ?? $"Пользователь #{t.AuthorId}") 
                            : $"Пользователь #{t.AuthorId}",
                        AssigneeId = t.AssigneeId,
                        AssigneeName = t.Assignee != null ? t.Assignee.FirstName + " " + t.Assignee.LastName : null,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        PlannedHours = t.PlannedHours ?? 0,
                        ActualHours = t.ActualHours ?? 0,
                        CommentsCount = t.Comments.Count
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Tasks = tasks,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки задач: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTask(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Author)
                .Include(t => t.Assignee)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            var commentsCount = await _context.Comments
                .Where(c => c.TaskId == id)
                .CountAsync();

            return new
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                ProjectId = task.ProjectId,
                ProjectName = task.Project?.Name ?? "Неизвестный проект",
                AuthorId = task.AuthorId,
                AuthorName = task.Author != null ? $"{task.Author.FirstName} {task.Author.LastName}" : "Неизвестен",
                AssigneeId = task.AssigneeId,
                AssigneeName = task.Assignee != null ? $"{task.Assignee.FirstName} {task.Assignee.LastName}" : null,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                PlannedHours = task.PlannedHours ?? 0,
                ActualHours = task.ActualHours ?? 0,
                CommentsCount = commentsCount
            };
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Task>>> GetTasksByStatus(int status)
        {
            return await _context.Tasks
                .Where(t => t.Status == status)
                .Include(t => t.Project)
                .Include(t => t.Assignee)
                .ToListAsync();
        }

        [HttpGet("priority/{priority}")]
        public async Task<ActionResult<IEnumerable<Task>>> GetTasksByPriority(int priority)
        {
            return await _context.Tasks
                .Where(t => t.Priority == priority)
                .Include(t => t.Project)
                .Include(t => t.Assignee)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<object>> PostTask(TaskCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var task = new Task
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                Priority = dto.Priority,
                ProjectId = dto.ProjectId,
                AuthorId = dto.AuthorId,
                AssigneeId = dto.AssigneeId,
                PlannedHours = dto.PlannedHours,
                ActualHours = dto.ActualHours,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTask", new { id = task.Id }, new
            {
                task.Id,
                task.Title,
                task.Description,
                task.Status,
                task.Priority,
                task.ProjectId,
                task.AuthorId,
                task.AssigneeId,
                task.PlannedHours,
                task.ActualHours,
                task.CreatedAt,
                task.UpdatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTask(int id, TaskCreateUpdateDto dto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Status = dto.Status;
            task.Priority = dto.Priority;
            task.ProjectId = dto.ProjectId;
            task.AuthorId = dto.AuthorId;
            task.AssigneeId = dto.AssigneeId;
            task.PlannedHours = dto.PlannedHours;
            task.ActualHours = dto.ActualHours;
            task.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
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

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] int status)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.Status = status;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}/assignee")]
        public async Task<IActionResult> UpdateTaskAssignee(int id, [FromBody] int assigneeId)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.AssigneeId = assigneeId;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}