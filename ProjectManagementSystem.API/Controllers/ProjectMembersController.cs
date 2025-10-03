using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Database.Data;
using ProjectManagementSystem.Database.Entities;

namespace ProjectManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectMembersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectMembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<object>> GetProjectMembers(int projectId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var baseQuery = _context.ProjectUsers
                .Where(pu => pu.ProjectId == projectId)
                .Include(pu => pu.User);

            var query = baseQuery.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(pu => 
                    pu.User.FirstName.Contains(search) || 
                    pu.User.LastName.Contains(search) || 
                    pu.User.Email.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var members = await query
                .Select(pu => new
                {
                    Id = pu.User.Id,
                    FirstName = pu.User.FirstName,
                    LastName = pu.User.LastName,
                    Email = pu.User.Email,
                    Role = pu.User.Role,
                    JoinedAt = pu.JoinedAt
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                Members = members,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            });
        }

        [HttpPost("project/{projectId}/user/{userId}")]
        public async Task<ActionResult> AddUserToProject(int projectId, int userId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound("Проект не найден");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            var existingMembership = await _context.ProjectUsers
                .FirstOrDefaultAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

            if (existingMembership != null)
            {
                return BadRequest("Пользователь уже является участником проекта");
            }

            var projectUser = new ProjectUser
            {
                ProjectId = projectId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
                RoleInProject = "Участник"
            };

            _context.ProjectUsers.Add(projectUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Пользователь добавлен в проект" });
        }

        [HttpDelete("project/{projectId}/user/{userId}")]
        public async Task<ActionResult> RemoveUserFromProject(int projectId, int userId)
        {
            var projectUser = await _context.ProjectUsers
                .FirstOrDefaultAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

            if (projectUser == null)
            {
                return NotFound("Пользователь не является участником проекта");
            }

            _context.ProjectUsers.Remove(projectUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Пользователь удален из проекта" });
        }

        [HttpGet("available-users/{projectId}")]
        public async Task<ActionResult<object>> GetAvailableUsers(int projectId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var projectMemberIds = await _context.ProjectUsers
                .Where(pu => pu.ProjectId == projectId)
                .Select(pu => pu.UserId)
                .ToListAsync();

            var query = _context.Users
                .Where(u => !projectMemberIds.Contains(u.Id));

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => 
                    u.FirstName.Contains(search) || 
                    u.LastName.Contains(search) || 
                    u.Email.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var availableUsers = await query
                .Select(u => new
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                Users = availableUsers,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            });
        }
    }
}
