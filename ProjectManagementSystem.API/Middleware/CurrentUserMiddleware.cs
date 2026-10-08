using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.API.Services;
using ProjectManagementSystem.Database.Data;

namespace ProjectManagementSystem.API.Middleware
{
    public class CurrentUserMiddleware
    {
        private readonly RequestDelegate _next;

        public CurrentUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ICurrentRequestUser currentUser, ApplicationDbContext db)
        {
            if (context.Request.Headers.TryGetValue("X-User-Id", out var headerValue)
                && int.TryParse(headerValue, out var userId)
                && userId > 0)
            {
                var user = await db.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    currentUser.UserId = user.Id;
                    currentUser.Login = user.Login;
                    currentUser.FullName = $"{user.FirstName} {user.LastName}".Trim();
                    currentUser.Role = user.Role;
                }
            }

            await _next(context);
        }
    }
}
