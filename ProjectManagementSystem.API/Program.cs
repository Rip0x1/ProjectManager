using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ProjectManagementSystem.API.Logging;
using ProjectManagementSystem.API.Middleware;
using ProjectManagementSystem.API.Services;
using ProjectManagementSystem.Database.Data;
using ProjectManagementSystem.Database.Entities;
using ProjectManagementSystem.API.Utilities;

namespace ProjectManagementSystem.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var logsPath = builder.Configuration["LogsPath"];
            if (string.IsNullOrWhiteSpace(logsPath))
            {
                logsPath = Path.Combine(builder.Environment.ContentRootPath, "logs");
            }
            Directory.CreateDirectory(logsPath);
            builder.Services.AddSingleton(new AuditFileWriter(logsPath));

            builder.Services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    opts.JsonSerializerOptions.WriteIndented = false;
                });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 25_000_000;
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentRequestUser, CurrentRequestUser>();
            builder.Services.AddScoped<IAuditService, AuditService>();
            builder.Services.AddScoped<AuditSaveChangesInterceptor>();

            builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("ProjectManagementSystem.Database")
                        .CommandTimeout(60)
                ).AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));

            var app = builder.Build();

            app.UseCors("AllowAll");
            app.UseMiddleware<CurrentUserMiddleware>();

            var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(uploadsPath),
                RequestPath = "/uploads"
            });

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var migrated = false;
                for (var attempt = 1; attempt <= 30; attempt++)
                {
                    try
                    {
                        context.Database.Migrate();
                        migrated = true;
                        break;
                    }
                    catch (Exception ex) when (attempt < 30)
                    {
                        logger.LogWarning(ex, "База данных недоступна, попытка {Attempt}/30", attempt);
                        Thread.Sleep(2000);
                    }
                }

                if (!migrated)
                {
                    context.Database.Migrate();
                }

                if (!context.Users.Any(u => u.Login == "admin"))
                {
                    var adminUser = new User
                    {
                        FirstName = "admin",
                        LastName = "admin",
                        Login = "admin",
                        PasswordHash = PasswordHasher.HashPassword("admin123"),
                        Role = 2,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Users.Add(adminUser);
                    context.SaveChanges();
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseHttpsRedirection();
            }

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
