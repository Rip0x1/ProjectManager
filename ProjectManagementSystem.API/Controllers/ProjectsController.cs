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

        // GET: api/Projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProjects()
        {
            var result = await _context.Projects
                .Include(p => p.Manager)
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
                .ToListAsync();

            return Ok(result);
        }

        // GET: api/Projects/5
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

        // POST: api/Projects
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

        // PUT: api/Projects/5
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

        // DELETE: api/Projects/5
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