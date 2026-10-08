using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.API.Services;
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

        public DataGeneratorController(ApplicationDbContext context, ILogger<DataGeneratorController> logger, ICurrentRequestUser currentUser)
        {
            _context = context;
            _logger = logger;
            currentUser.SuppressAudit = true;
        }

        [HttpPost("generate-small")]
        public async Task<ActionResult> GenerateSmallDataset()
        {
            try
            {
                _logger.LogInformation("Начало генерации малого набора данных (100 записей)...");

                await WipeDatabaseKeepAdminAsync();
                await CreateAdminUser();

                var results = new
                {
                    Users = await GenerateUsers(10),
                    Projects = await GenerateProjects(5), 
                    ProjectUsers = await GenerateProjectUsers(),
                    Tasks = await GenerateTasks(50), 
                    Comments = await GenerateComments(35) 
                };

                _logger.LogInformation("Малый набор данных успешно загружен (≈100 записей)");
                return Ok(new
                {
                    Message = "Малый набор данных успешно загружен (≈100 записей)",
                    Results = results,
                    TotalRecords = results.Users + results.Projects + results.ProjectUsers + results.Tasks + results.Comments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации малого набора данных");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("generate-medium")]
        public async Task<ActionResult> GenerateMediumDataset()
        {
            try
            {
                _logger.LogInformation("Начало генерации среднего набора данных (1 000 записей)...");

                await WipeDatabaseKeepAdminAsync();
                await CreateAdminUser();

                var results = new
                {
                    Users = await GenerateUsers(50),
                    Projects = await GenerateProjects(25), 
                    ProjectUsers = await GenerateProjectUsers(),
                    Tasks = await GenerateTasks(500), 
                    Comments = await GenerateComments(400) 
                };

                _logger.LogInformation("Средний набор данных успешно загружен (≈1 000 записей)");
                return Ok(new
                {
                    Message = "Средний набор данных успешно загружен (≈1 000 записей)",
                    Results = results,
                    TotalRecords = results.Users + results.Projects + results.ProjectUsers + results.Tasks + results.Comments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации среднего набора данных");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("generate-10k")]
        public async Task<ActionResult> Generate10kDataset()
        {
            try
            {
                _logger.LogInformation("Начало генерации набора данных (10 000 записей)...");

                await WipeDatabaseKeepAdminAsync();
                await CreateAdminUser();

                var results = new
                {
                    Users = await GenerateUsers(200),
                    Projects = await GenerateProjects(100), 
                    ProjectUsers = await GenerateProjectUsers(),
                    Tasks = await GenerateTasks(5000), 
                    Comments = await GenerateComments(4700) 
                };

                _logger.LogInformation("Набор данных успешно загружен (≈10 000 записей)");
                return Ok(new
                {
                    Message = "Набор данных успешно загружен (≈10 000 записей)",
                    Results = results,
                    TotalRecords = results.Users + results.Projects + results.ProjectUsers + results.Tasks + results.Comments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации набора 10k данных");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("generate-20k")]
        public async Task<ActionResult> Generate20kDataset()
        {
            try
            {
                _logger.LogInformation("Начало генерации набора данных (20 000 записей)...");

                await WipeDatabaseKeepAdminAsync();
                await CreateAdminUser();

                var results = new
                {
                    Users = await GenerateUsers(400),
                    Projects = await GenerateProjects(200), 
                    ProjectUsers = await GenerateProjectUsers(),
                    Tasks = await GenerateTasks(10000), 
                    Comments = await GenerateComments(9400) 
                };

                _logger.LogInformation("Набор данных успешно загружен (≈20 000 записей)");
                return Ok(new
                {
                    Message = "Набор данных успешно загружен (≈20 000 записей)",
                    Results = results,
                    TotalRecords = results.Users + results.Projects + results.ProjectUsers + results.Tasks + results.Comments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации набора 20k данных");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("generate-50k")]
        public async Task<ActionResult> Generate50kDataset()
        {
            try
            {
                _logger.LogInformation("Начало генерации набора данных (50 000 записей)...");

                await WipeDatabaseKeepAdminAsync();
                await CreateAdminUser();

                var results = new
                {
                    Users = await GenerateUsers(1000),
                    Projects = await GenerateProjects(500), 
                    ProjectUsers = await GenerateProjectUsers(),
                    Tasks = await GenerateTasks(25000), 
                    Comments = await GenerateComments(23500) 
                };

                _logger.LogInformation("Набор данных успешно загружен (≈50 000 записей)");
                return Ok(new
                {
                    Message = "Набор данных успешно загружен (≈50 000 записей)",
                    Results = results,
                    TotalRecords = results.Users + results.Projects + results.ProjectUsers + results.Tasks + results.Comments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации набора 50k данных");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("generate-100k")]
        public async Task<ActionResult> Generate100kDataset()
        {
            try
            {
                _logger.LogInformation("Начало генерации набора данных (100 000 записей)...");

                await WipeDatabaseKeepAdminAsync();
                await CreateAdminUser();

                var results = new
                {
                    Users = await GenerateUsers(2000),
                    Projects = await GenerateProjects(1000), 
                    ProjectUsers = await GenerateProjectUsers(),
                    Tasks = await GenerateTasks(50000), 
                    Comments = await GenerateComments(47000) 
                };

                _logger.LogInformation("Набор данных успешно загружен (≈100 000 записей)");
                return Ok(new
                {
                    Message = "Набор данных успешно загружен (≈100 000 записей)",
                    Results = results,
                    TotalRecords = results.Users + results.Projects + results.ProjectUsers + results.Tasks + results.Comments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации набора 100k данных");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("clear")]
        public async Task<ActionResult> ClearDatabase()
        {
            _logger.LogInformation("Очистка базы данных...");
            await WipeDatabaseKeepAdminAsync();
            await CreateAdminUser();
            _logger.LogInformation("База данных была успешно очищена, администратор сохранён");
            return Ok("База данных успешно очищена, учётная запись admin сохранена");
        }

        private async System.Threading.Tasks.Task WipeDatabaseKeepAdminAsync()
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
            _context.ChangeTracker.Clear();

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM CommentAttachments");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Comments");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM TaskAttachments");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM TaskAssignees");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Tasks");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProjectAttachments");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProjectUsers");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Projects");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM AuditLogs");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Users WHERE [Login] <> N'admin'");

            _context.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        private async System.Threading.Tasks.Task CreateAdminUser()
        {
            if (!await _context.Users.AnyAsync(u => u.Login == "admin"))
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

                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Администратор был создан: admin / admin123");
            }
        }

        private async Task<int> GenerateUsers(int count)
        {
            _logger.LogInformation($"Генерация {count} пользователей...");

            var users = new List<User>();
            var userFaker = new Bogus.Faker<User>()
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Login, (f, u) => (u.FirstName + u.LastName + f.Random.Number(100, 999)).ToLower())
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
                _logger.LogInformation($"Генерация {i + batchSize}/{count} пользователей");
            }

            return users.Count;
        }

        private async Task<int> GenerateProjects(int count)
        {
            _logger.LogInformation($"Генерация {count} проектов...");

            var users = await _context.Users.ToListAsync();
            var managers = users.Where(u => u.Role >= 1).ToList();

            if (!managers.Any()) managers = users.Take(10).ToList();

            var projects = new List<Project>();
            var projectFaker = new Bogus.Faker<Project>()
                .RuleFor(p => p.Name, f => $"{f.Commerce.ProductName()} {f.Commerce.ProductAdjective()} Project")
                .RuleFor(p => p.Description, f => f.Lorem.Paragraphs(1, 3))
                .RuleFor(p => p.ManagerId, f => f.PickRandom(managers).Id)
                .RuleFor(p => p.Status, f => f.Random.Int(0, 2))
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
                _logger.LogInformation($"Генерация {i + batchSize}/{count} проектов");
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
                        RoleInProject = new Bogus.Faker().PickRandom(roles),
                        JoinedAt = new Bogus.Faker().Date.Between(project.CreatedAt, DateTime.UtcNow)
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
                .RuleFor(t => t.Status, f => f.Random.Int(0, 5))
                .RuleFor(t => t.Priority, f => f.Random.Int(0, 2))
                .RuleFor(t => t.ProjectId, f => f.Random.Bool(0.85f) ? f.PickRandom(projects).Id : (int?)null)
                .RuleFor(t => t.AuthorId, f => f.PickRandom(users).Id)
                .RuleFor(t => t.AssigneeId, f => f.PickRandom(users).Id)
                .RuleFor(t => t.CreatedAt, f => f.Date.Past(1))
                .RuleFor(t => t.UpdatedAt, (f, t) => f.Date.Between(t.CreatedAt, DateTime.Now))
                .RuleFor(t => t.DueDate, (f, t) => f.Date.Between(t.CreatedAt, t.CreatedAt.AddDays(30)))
                .RuleFor(t => t.CompletedAt, (f, t) =>
                    t.Status == 3 ? f.Date.Between(t.CreatedAt, DateTime.Now) : (DateTime?)null);

            for (int i = 0; i < count; i += 1000)
            {
                var batchSize = Math.Min(1000, count - i);
                var batch = taskFaker.Generate(batchSize);

                await _context.Tasks.AddRangeAsync(batch);
                await _context.SaveChangesAsync();

                var assignments = batch
                    .Where(t => t.AssigneeId.HasValue)
                    .Select(t => new TaskAssignee { TaskId = t.Id, UserId = t.AssigneeId!.Value })
                    .ToList();
                if (assignments.Count > 0)
                {
                    await _context.TaskAssignees.AddRangeAsync(assignments);
                    await _context.SaveChangesAsync();
                }

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

        [HttpPost("generate-test")]
        public async Task<ActionResult> GenerateTestData()
        {
            try
            {
                _logger.LogInformation("Начало генерации тестовых данных для проверки...");

                await WipeDatabaseKeepAdminAsync();
                await CreateAdminUser();

                var results = new
                {
                    Users = await GenerateUsers(10),
                    Projects = await GenerateProjects(5),
                    ProjectUsers = await GenerateProjectUsers(),
                    Tasks = await GenerateTasks(20),
                    Comments = await GenerateComments(50)
                };

                _logger.LogInformation("Тестовые данные успешно загружены");
                return Ok(new
                {
                    Message = "Тестовые данные успешно загружены",
                    Results = results,
                    TotalRecords = results.Users + results.Projects + results.ProjectUsers + results.Tasks + results.Comments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации тестовых данных");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}