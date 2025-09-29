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

        // GET: api/Tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasks([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var tasks = await _context.Tasks
                    .OrderByDescending(t => t.CreatedAt)
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
                        ProjectName = t.Project.Name,
                        AuthorId = t.AuthorId,
                        AuthorName = t.Author.FirstName + " " + t.Author.LastName,
                        AssigneeId = t.AssigneeId,
                        AssigneeName = t.Assignee != null ? t.Assignee.FirstName + " " + t.Assignee.LastName : null,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        PlannedHours = t.PlannedHours,
                        ActualHours = t.ActualHours,
                        CommentsCount = 0
                    })
                    .ToListAsync();

                return tasks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки задач: {ex.Message}");
                return new List<TaskResponseDto>();
            }
        }

        // GET: api/Tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Task>> GetTask(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Author)
                .Include(t => t.Assignee)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            return task;
        }

        // GET: api/Tasks/status/1
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Task>>> GetTasksByStatus(int status)
        {
            return await _context.Tasks
                .Where(t => t.Status == status)
                .Include(t => t.Project)
                .Include(t => t.Assignee)
                .ToListAsync();
        }

        // GET: api/Tasks/priority/2
        [HttpGet("priority/{priority}")]
        public async Task<ActionResult<IEnumerable<Task>>> GetTasksByPriority(int priority)
        {
            return await _context.Tasks
                .Where(t => t.Priority == priority)
                .Include(t => t.Project)
                .Include(t => t.Assignee)
                .ToListAsync();
        }

        // POST: api/Tasks
        [HttpPost]
        public async Task<ActionResult<Task>> PostTask(Task task)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTask", new { id = task.Id }, task);
        }

        // PUT: api/Tasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTask(int id, Task task)
        {
            if (id != task.Id)
            {
                return BadRequest();
            }

            task.UpdatedAt = DateTime.UtcNow;
            _context.Entry(task).State = EntityState.Modified;

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

        // PATCH: api/Tasks/5/status
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

        // PATCH: api/Tasks/5/assignee
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

        // DELETE: api/Tasks/5
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