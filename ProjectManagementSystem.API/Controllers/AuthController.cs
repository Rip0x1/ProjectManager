using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.API.Models.DTOs;
using ProjectManagementSystem.API.Services;
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
        private readonly IAuditService _auditService;
        private readonly ICurrentRequestUser _currentUser;

        public AuthController(ApplicationDbContext context, IAuditService auditService, ICurrentRequestUser currentUser)
        {
            _context = context;
            _auditService = auditService;
            _currentUser = currentUser;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(UserAuthDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Login == registerDto.Login))
            {
                return BadRequest(new AuthResponseDto { Message = "Данный логин уже используется" });
            }

            var isFirstUser = !await _context.Users.AnyAsync();
            var userRole = isFirstUser ? 2 : registerDto.Role;

            var user = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Login = registerDto.Login,
                PasswordHash = PasswordHasher.HashPassword(registerDto.Password),
                Role = userRole,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                UserId = user.Id,
                Login = user.Login,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Message = "Регистрация пользователя прошла успешно"
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(UserLoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == loginDto.Login);

            if (user == null)
            {
                await _auditService.LogAsync(
                    "LoginFailed",
                    "User",
                    null,
                    loginDto.Login,
                    null,
                    loginDto.Login,
                    string.Empty,
                    null,
                    null,
                    GetIpAddress(),
                    GetUserAgent(),
                    $"Неудачная попытка входа: аккаунт {loginDto.Login} не найден");
                return Unauthorized(new AuthResponseDto { Message = "Неверный логин или пароль" });
            }

            if (!PasswordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                await _auditService.LogAsync(
                    "LoginFailed",
                    "User",
                    user.Id,
                    $"{user.FirstName} {user.LastName}".Trim(),
                    user.Id,
                    user.Login,
                    $"{user.FirstName} {user.LastName}".Trim(),
                    null,
                    null,
                    GetIpAddress(),
                    GetUserAgent(),
                    $"Неудачная попытка входа: неверный пароль для {user.Login}");
                return Unauthorized(new AuthResponseDto { Message = "Неверный логин или пароль" });
            }

            await _auditService.LogAsync(
                "Login",
                "User",
                user.Id,
                $"{user.FirstName} {user.LastName}".Trim(),
                user.Id,
                user.Login,
                $"{user.FirstName} {user.LastName}".Trim(),
                null,
                null,
                GetIpAddress(),
                GetUserAgent(),
                $"{user.FirstName} {user.LastName}".Trim() + " вошёл в систему");

            return Ok(new AuthResponseDto
            {
                UserId = user.Id,
                Login = user.Login,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Message = "Авторизация прошла успешно"
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (_currentUser.UserId.HasValue)
            {
                await _auditService.LogAsync(
                    "Logout",
                    "User",
                    _currentUser.UserId,
                    _currentUser.FullName,
                    _currentUser.UserId,
                    _currentUser.Login,
                    _currentUser.FullName,
                    null,
                    null,
                    GetIpAddress(),
                    GetUserAgent(),
                    $"{_currentUser.FullName} вышел из системы");
            }

            return Ok(new { Message = "Выход выполнен" });
        }

        private string GetIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }

        private string GetUserAgent()
        {
            return HttpContext.Request.Headers.UserAgent.ToString();
        }
    }
}