using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.API.Models.DTOs;
using ProjectManagementSystem.Database.Data;
using ProjectManagementSystem.Database.Entities;

namespace ProjectManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectUsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProjectUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var projectUsers = await _context.ProjectUsers
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(x => new ProjectUserResponseDto
    {
        Id = x.Id,
        ProjectId = x.ProjectId,
        UserId = x.UserId,
        RoleInProject = x.RoleInProject,
        ProjectsAuthorResponceDto = new ProjectsAuthorResponceDto
        {
            FirstName = x.User.FirstName,
            LastName = x.User.LastName,
            Email = x.User.Email,
        },
        ProjectsResponceDto = new ProjectsResponceDto
        {
            Id = x.Project.Id,
            Name = x.Project.Name,
            Description = x.Project.Description
        }
    })
    .ToListAsync();
                return projectUsers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки проектов пользователей: {ex.Message}");
                return new List<ProjectUserResponseDto>();
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectUser>> GetProjectUser(int id)
        {
            var projectUser = await _context.ProjectUsers
                .Include(pu => pu.Project)
                .Include(pu => pu.User)
                .FirstOrDefaultAsync(pu => pu.Id == id);

            if (projectUser == null)
            {
                return NotFound();
            }

            return projectUser;
        }

        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<ProjectUser>>> GetProjectUsersByProject(int projectId)
        {
            return await _context.ProjectUsers
                .Where(pu => pu.ProjectId == projectId)
                .Include(pu => pu.User)
                .ToListAsync();
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ProjectUser>>> GetProjectUsersByUser(int userId)
        {
            return await _context.ProjectUsers
                .Where(pu => pu.UserId == userId)
                .Include(pu => pu.Project)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<ProjectUser>> PostProjectUser(ProjectUser projectUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _context.ProjectUsers
                .FirstOrDefaultAsync(pu => pu.ProjectId == projectUser.ProjectId && pu.UserId == projectUser.UserId);

            if (existing != null)
            {
                return Conflict("User is already assigned to this project");
            }

            _context.ProjectUsers.Add(projectUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProjectUser", new { id = projectUser.Id }, projectUser);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProjectUser(int id, ProjectUser projectUser)
        {
            if (id != projectUser.Id)
            {
                return BadRequest();
            }

            _context.Entry(projectUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectUserExists(id))
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectUser(int id)
        {
            var projectUser = await _context.ProjectUsers.FindAsync(id);
            if (projectUser == null)
            {
                return NotFound();
            }

            _context.ProjectUsers.Remove(projectUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("project/{projectId}/user/{userId}")]
        public async Task<IActionResult> DeleteProjectUserByProjectAndUser(int projectId, int userId)
        {
            var projectUser = await _context.ProjectUsers
                .FirstOrDefaultAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

            if (projectUser == null)
            {
                return NotFound();
            }

            _context.ProjectUsers.Remove(projectUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProjectUserExists(int id)
        {
            return _context.ProjectUsers.Any(e => e.Id == id);
        }
    }
}