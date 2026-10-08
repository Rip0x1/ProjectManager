using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.API.Utilities;
using ProjectManagementSystem.API.Models.DTOs;
using ProjectManagementSystem.Database.Data;
using ProjectManagementSystem.Database.Entities;
using TaskEntity = ProjectManagementSystem.Database.Entities.Task;

namespace ProjectManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            var users = await _context.Users
                .Select(MapUserToDto)
                .ToListAsync();

            return users;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(MapUserToDto)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> PostUser(UserCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _context.Users.AnyAsync(u => u.Login == dto.Login))
            {
                return BadRequest(new { Message = "Данный логин уже используется" });
            }

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Login = dto.Login,
                PasswordHash = PasswordHasher.HashPassword(dto.Password),
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Login = user.Login,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                ManagedProjectsCount = 0,
                AuthoredTasksCount = 0,
                AssignedTasksCount = 0,
                CommentsCount = 0
            };

            return CreatedAtAction("GetUser", new { id = user.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponseDto>> PutUser(int id, UserUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (await _context.Users.AnyAsync(u => u.Login == dto.Login && u.Id != id))
            {
                return BadRequest(new { Message = "Данный логин уже используется" });
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Login = dto.Login;
            user.Role = dto.Role;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = PasswordHasher.HashPassword(dto.Password);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            var response = await _context.Users
                .Where(u => u.Id == id)
                .Select(MapUserToDto)
                .FirstAsync();

            return Ok(response);
        }

        [HttpGet("{id}/projects")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserProjects(int id)
        {
            if (!UserExists(id))
            {
                return NotFound();
            }

            var projects = await _context.Projects
                .AsNoTracking()
                .OrderByDescending(p => p.ManagerId == id)
                .ThenByDescending(p => p.ProjectUsers.Any(pu => pu.UserId == id))
                .ThenByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Status,
                    p.Deadline,
                    p.CreatedAt,
                    p.ManagerId,
                    ManagerName = p.Manager == null
                        ? string.Empty
                        : (p.Manager.FirstName + " " + p.Manager.LastName).Trim(),
                    IsManager = p.ManagerId == id,
                    IsMember = p.ProjectUsers.Any(pu => pu.UserId == id)
                })
                .ToListAsync();

            return Ok(projects);
        }

        [HttpGet("{id}/authored-tasks")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserAuthoredTasks(int id)
        {
            if (!UserExists(id))
            {
                return NotFound();
            }

            return Ok(await MapUserTasksAsync(_context.Tasks.Where(t => t.AuthorId == id)));
        }

        [HttpGet("{id}/assigned-tasks")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserAssignedTasks(int id)
        {
            if (!UserExists(id))
            {
                return NotFound();
            }

            return Ok(await MapUserTasksAsync(_context.Tasks.Where(t =>
                t.AssigneeId == id || t.Assignees.Any(a => a.UserId == id))));
        }

        [HttpGet("{id}/comments")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserComments(int id)
        {
            if (!UserExists(id))
            {
                return NotFound();
            }

            var comments = await _context.Comments
                .AsNoTracking()
                .Where(c => c.AuthorId == id)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreatedAt,
                    c.TaskId,
                    TaskTitle = c.Task != null ? c.Task.Title : null,
                    ProjectName = c.Task != null && c.Task.Project != null ? c.Task.Project.Name : null
                })
                .ToListAsync();

            return Ok(comments);
        }

        private static async Task<List<object>> MapUserTasksAsync(IQueryable<TaskEntity> query)
        {
            var rows = await query
                .AsNoTracking()
                .OrderByDescending(t => t.UpdatedAt)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.Priority,
                    t.ProjectId,
                    ProjectName = t.Project != null ? t.Project.Name : "Без проекта",
                    t.CreatedAt,
                    t.DueDate
                })
                .ToListAsync();

            return rows.Cast<object>().ToList();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var managedProjectsCount = await _context.Projects.CountAsync(p => p.ManagerId == id);
                if (managedProjectsCount > 0)
                {
                    return BadRequest("Нельзя удалить пользователя, который управляет проектами. Сначала переназначьте проекты другому менеджеру.");
                }

                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Role == 2);
                
                if (adminUser != null)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE TaskAssignees SET UserId = {0} WHERE UserId = {1} AND NOT EXISTS (SELECT 1 FROM TaskAssignees ta WHERE ta.TaskId = TaskAssignees.TaskId AND ta.UserId = {0})",
                        adminUser.Id, id);
                    await _context.Database.ExecuteSqlRawAsync("DELETE FROM TaskAssignees WHERE UserId = {0}", id);
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE Tasks SET AssigneeId = {0} WHERE AssigneeId = {1}", 
                        adminUser.Id, id);
                    
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE Tasks SET AuthorId = {0} WHERE AuthorId = {1}", 
                        adminUser.Id, id);
                }
                else
                {
                    await _context.Database.ExecuteSqlRawAsync("DELETE FROM TaskAssignees WHERE UserId = {0}", id);
                    await _context.Database.ExecuteSqlRawAsync("DELETE FROM Tasks WHERE AssigneeId = {0}", id);
                    await _context.Database.ExecuteSqlRawAsync("DELETE FROM Tasks WHERE AuthorId = {0}", id);
                }

                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Comments WHERE AuthorId = {0}", id);

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка удаления пользователя: {ex.Message}");
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        private static readonly System.Linq.Expressions.Expression<Func<User, UserResponseDto>> MapUserToDto = u => new UserResponseDto
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Login = u.Login,
            Role = u.Role,
            CreatedAt = u.CreatedAt,
            ManagedProjectsCount = u.ManagedProjects.Count,
            AuthoredTasksCount = u.AuthoredTasks.Count,
            AssignedTasksCount = u.TaskAssignments.Count + u.AssignedTasks.Count(t => !u.TaskAssignments.Any(ta => ta.TaskId == t.Id)),
            CommentsCount = u.Comments.Count
        };
    }
}