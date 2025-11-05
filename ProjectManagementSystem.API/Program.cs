using Microsoft.EntityFrameworkCore;
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


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("ProjectManagementSystem.Database")
                        .CommandTimeout(60) 
                ));

            var app = builder.Build();

            app.Urls.Add("http://*:7260");
            app.Urls.Add("https://localhost:7260");

            app.UseCors("AllowAll");

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();

                if (!context.Users.Any())
                {
                    var adminUser = new User
                    {
                        FirstName = "admin",
                        LastName = "admin",
                        Email = "admin@admin.com",
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
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
