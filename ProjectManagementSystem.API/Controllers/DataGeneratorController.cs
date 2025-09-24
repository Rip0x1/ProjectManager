using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.API.Utilities;
using ProjectManagementSystem.Database.Data;
using ProjectManagementSystem.Database.Entities;
using Task = ProjectManagementSystem.Database.Entities.Task;

namespace ProjectManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataGeneratorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DataGeneratorController> _logger;

        public DataGeneratorController(ApplicationDbContext context, ILogger<DataGeneratorController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("generate-large")]
        public async Task<ActionResult> GenerateLargeDataset()
        {
            try
            {
                _logger.LogInformation("Starting LARGE test data generation...");

                await ClearDatabase();

                var results = new
                {
                    Users = await GenerateUsers(1000),
                    Projects = await GenerateProjects(500), 
                    ProjectUsers = await GenerateProjectUsers(),
                    Tasks = await GenerateTasks(50000), 
                    Comments = await GenerateComments(100000) 
                };

                _logger.LogInformation("LARGE test data generation completed");
                return Ok(new
                {
                    Message = "Large dataset generated successfully",
                    Results = results,
                    TotalRecords = results.Users + results.Projects + results.ProjectUsers + results.Tasks + results.Comments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during large data generation");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("generate-massive")]
        public async Task<ActionResult> GenerateMassiveDataset()
        {
            try
            {
                _logger.LogInformation("Starting MASSIVE test data generation...");

                await ClearDatabase();

                var results = new
                {
                    Users = await GenerateUsers(2000), 
                    Projects = await GenerateProjects(1000), 
                    ProjectUsers = await GenerateProjectUsers(),
                    Tasks = await GenerateTasks(100000), 
                    Comments = await GenerateComments(200000)
                };

                _logger.LogInformation("MASSIVE test data generation completed");
                return Ok(new
                {
                    Message = "Massive dataset generated successfully",
                    Results = results,
                    TotalRecords = results.Users + results.Projects + results.ProjectUsers + results.Tasks + results.Comments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during massive data generation");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("clear")]
        public async Task<ActionResult> ClearDatabase()
        {
            _logger.LogInformation("Clearing database...");

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Comments");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Tasks");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProjectUsers");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Projects");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Users");

            _context.ChangeTracker.AutoDetectChangesEnabled = true;

            _logger.LogInformation("Database cleared successfully");
            return Ok("Database cleared successfully");
        }

        private async Task<int> GenerateUsers(int count)
        {
            _logger.LogInformation($"Generating {count} users...");

            var users = new List<User>();
            var userFaker = new Bogus.Faker<User>()
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName).ToLower())
                .RuleFor(u => u.PasswordHash, f =>
                {
                    var passwordLength = f.Random.Int(5, 10);
                    var password = f.Random.String2(passwordLength, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
                    return PasswordHasher.HashPassword(password);
                })
                .RuleFor(u => u.Role, f => f.Random.Int(0, 2))
                .RuleFor(u => u.CreatedAt, f => f.Date.Past(3));

            for (int i = 0; i < count; i += 1000)
            {
                var batchSize = Math.Min(1000, count - i);
                var batch = userFaker.Generate(batchSize);

                await _context.Users.AddRangeAsync(batch);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();

                users.AddRange(batch);
                _logger.LogInformation($"Generated {i + batchSize}/{count} users");
            }

            return users.Count;
        }

        private async Task<int> GenerateProjects(int count)
        {
            _logger.LogInformation($"Generating {count} projects...");

            var users = await _context.Users.ToListAsync();
            var managers = users.Where(u => u.Role >= 1).ToList();

            if (!managers.Any()) managers = users.Take(10).ToList();

            var projects = new List<Project>();
            var projectFaker = new Bogus.Faker<Project>()
                .RuleFor(p => p.Name, f => $"{f.Commerce.ProductName()} {f.Commerce.ProductAdjective()} Project")
                .RuleFor(p => p.Description, f => f.Lorem.Paragraphs(1, 3))
                .RuleFor(p => p.ManagerId, f => f.PickRandom(managers).Id)
                .RuleFor(p => p.CreatedAt, f => f.Date.Past(2))
                .RuleFor(p => p.Deadline, f => f.Date.Between(DateTime.Now, DateTime.Now.AddYears(1)));

            for (int i = 0; i < count; i += 500)
            {
                var batchSize = Math.Min(500, count - i);
                var batch = projectFaker.Generate(batchSize);

                await _context.Projects.AddRangeAsync(batch);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
                projects.AddRange(batch);
                _logger.LogInformation($"Generated {i + batchSize}/{count} projects");
            }

            return projects.Count;
        }

        private async Task<int> GenerateProjectUsers()
        {
            _logger.LogInformation("Generating project-user relationships...");

            var projects = await _context.Projects.ToListAsync();
            var users = await _context.Users.ToListAsync();
            var projectUsers = new List<ProjectUser>();
            var roles = new[] { "Developer", "Designer", "Tester", "Analyst", "Architect", "Team Lead", "QA" };

            foreach (var project in projects)
            {
                var participantCount = new Random().Next(10, 50);
                var participants = users.OrderBy(x => Guid.NewGuid()).Take(participantCount).ToList();

                foreach (var user in participants)
                {
                    projectUsers.Add(new ProjectUser
                    {
                        ProjectId = project.Id,
                        UserId = user.Id,
                        RoleInProject = new Bogus.Faker().PickRandom(roles)
                    });
                }

                if (projectUsers.Count >= 1000)
                {
                    await _context.ProjectUsers.AddRangeAsync(projectUsers);
                    await _context.SaveChangesAsync();
                    _context.ChangeTracker.Clear();
                    projectUsers.Clear();
                }
            }

            if (projectUsers.Any())
            {
                await _context.ProjectUsers.AddRangeAsync(projectUsers);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Generated project-user relationships");
            return await _context.ProjectUsers.CountAsync();
        }

        private async Task<int> GenerateTasks(int count)
        {
            _logger.LogInformation($"Generating {count} tasks...");

            var projects = await _context.Projects.ToListAsync();
            var users = await _context.Users.ToListAsync();
            var tasks = new List<Task>();

            var taskFaker = new Bogus.Faker<Task>()
                .RuleFor(t => t.Title, f => f.Lorem.Sentence(3, 8))
                .RuleFor(t => t.Description, f => f.Lorem.Paragraphs(1, 2))
                .RuleFor(t => t.Status, f => f.Random.Int(0, 3))
                .RuleFor(t => t.Priority, f => f.Random.Int(0, 2))
                .RuleFor(t => t.ProjectId, f => f.PickRandom(projects).Id)
                .RuleFor(t => t.AuthorId, f => f.PickRandom(users).Id)
                .RuleFor(t => t.AssigneeId, f => f.PickRandom(users).Id)
                .RuleFor(t => t.CreatedAt, f => f.Date.Past(1))
                .RuleFor(t => t.UpdatedAt, (f, t) => f.Date.Between(t.CreatedAt, DateTime.Now))
                .RuleFor(t => t.PlannedHours, f => Math.Round(f.Random.Decimal(1, 40), 1))
                .RuleFor(t => t.ActualHours, (f, t) =>
                    t.Status == 3 ? Math.Round(f.Random.Decimal(t.PlannedHours * 0.5m, t.PlannedHours * 1.5m), 1) : (decimal?)null);

            for (int i = 0; i < count; i += 1000)
            {
                var batchSize = Math.Min(1000, count - i);
                var batch = taskFaker.Generate(batchSize);

                await _context.Tasks.AddRangeAsync(batch);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
                tasks.AddRange(batch);
                _logger.LogInformation($"Generated {i + batchSize}/{count} tasks");
            }

            return tasks.Count;
        }

        private async Task<int> GenerateComments(int count)
        {
            _logger.LogInformation($"Generating {count} comments...");

            var tasks = await _context.Tasks.ToListAsync();
            var users = await _context.Users.ToListAsync();
            var comments = new List<Comment>();

            var commentFaker = new Bogus.Faker<Comment>()
                .RuleFor(c => c.Content, f => f.Lorem.Paragraph())
                .RuleFor(c => c.TaskId, f => f.PickRandom(tasks).Id)
                .RuleFor(c => c.AuthorId, f => f.PickRandom(users).Id)
                .RuleFor(c => c.CreatedAt, f => f.Date.Past(1));

            for (int i = 0; i < count; i += 2000)
            {
                var batchSize = Math.Min(2000, count - i);
                var batch = commentFaker.Generate(batchSize);

                await _context.Comments.AddRangeAsync(batch);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
                comments.AddRange(batch);
                _logger.LogInformation($"Generated {i + batchSize}/{count} comments");
            }

            return comments.Count;
        }

        [HttpGet("stats")]
        public async Task<ActionResult> GetDatabaseStats()
        {
            var stats = new
            {
                Users = await _context.Users.CountAsync(),
                Projects = await _context.Projects.CountAsync(),
                ProjectUsers = await _context.ProjectUsers.CountAsync(),
                Tasks = await _context.Tasks.CountAsync(),
                Comments = await _context.Comments.CountAsync(),
                TotalRecords = await _context.Users.CountAsync() +
                             await _context.Projects.CountAsync() +
                             await _context.ProjectUsers.CountAsync() +
                             await _context.Tasks.CountAsync() +
                             await _context.Comments.CountAsync()
            };

            return Ok(stats);
        }
    }
}