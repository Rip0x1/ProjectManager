using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.API.Models.DTOs;
using ProjectManagementSystem.API.Utilities;
using ProjectManagementSystem.Database.Data;
using ProjectManagementSystem.Database.Entities;

namespace ProjectManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(UserAuthDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest(new AuthResponseDto { Message = "User with this email already exists" });
            }

            var user = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                PasswordHash = PasswordHasher.HashPassword(registerDto.Password),
                Role = registerDto.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Message = "User registered successfully"
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(UserLoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                return Unauthorized(new AuthResponseDto { Message = "Invalid email or password" });
            }

            if (!PasswordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized(new AuthResponseDto { Message = "Invalid email or password" });
            }

            return Ok(new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Message = "Login successful"
            });
        }
    }
}