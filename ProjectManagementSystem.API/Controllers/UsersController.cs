using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.API.Utilities;
using ProjectManagementSystem.API.Models.DTOs;
using ProjectManagementSystem.Database.Data;
using ProjectManagementSystem.Database.Entities;

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
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    ManagedProjectsCount = u.ManagedProjects.Count,
                    AuthoredTasksCount = u.AuthoredTasks.Count,
                    AssignedTasksCount = u.AssignedTasks.Count,
                    CommentsCount = u.Comments.Count
                })
                .ToListAsync();

            return users;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    ManagedProjectsCount = u.ManagedProjects.Count,
                    AuthoredTasksCount = u.AuthoredTasks.Count,
                    AssignedTasksCount = u.AssignedTasks.Count,
                    CommentsCount = u.Comments.Count
                })
                .FirstOrDefaultAsync(u => u.Id == id);

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


            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
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
                Email = user.Email,
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

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
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

            var response = new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                ManagedProjectsCount = user.ManagedProjects.Count,
                AuthoredTasksCount = user.AuthoredTasks.Count,
                AssignedTasksCount = user.AssignedTasks.Count,
                CommentsCount = user.Comments.Count
            };

            return Ok(response);
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
                        "UPDATE Tasks SET AssigneeId = {0} WHERE AssigneeId = {1}", 
                        adminUser.Id, id);
                    
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE Tasks SET AuthorId = {0} WHERE AuthorId = {1}", 
                        adminUser.Id, id);
                }
                else
                {
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
    }
}