using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Database.Data;
using ProjectManagementSystem.Database.Entities;
using ProjectManagementSystem.API.Models.DTOs;
using ProjectManagementSystem.API.Utilities;

namespace ProjectManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".doc", ".docx", ".pdf", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
        };

        public ProjectsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProjects()
        {
            var result = await _context.Projects
                .AsNoTracking()
                .Select(project => new
                {
                    project.Id,
                    project.Name,
                    project.Description,
                    project.ManagerId,
                    project.Status,
                    project.CreatedAt,
                    project.Deadline,
                    ParticipantsCount = project.ProjectUsers.Count,
                    TasksCount = project.Tasks.Count,
                    CommentsCount = _context.Comments.Count(c => c.Task != null && c.Task.ProjectId == project.Id),
                    Manager = project.Manager == null ? null : new
                    {
                        project.Manager.Id,
                        project.Manager.FirstName,
                        project.Manager.LastName,
                        project.Manager.Login,
                        project.Manager.Role
                    }
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Manager)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            var participantsCount = await _context.ProjectUsers
                .Where(pu => pu.ProjectId == id)
                .CountAsync();

            var tasksCount = await _context.Tasks
                .Where(t => t.ProjectId == id)
                .CountAsync();

            var taskIds = await _context.Tasks
                .Where(t => t.ProjectId == id)
                .Select(t => t.Id)
                .ToListAsync();

            var commentsCount = await _context.Comments
                .Where(c => c.TaskId.HasValue && taskIds.Contains(c.TaskId.Value))
                .CountAsync();

            return Ok(new
            {
                project.Id,
                project.Name,
                project.Description,
                project.ManagerId,
                project.Status,
                project.CreatedAt,
                project.Deadline,
                ParticipantsCount = participantsCount,
                TasksCount = tasksCount,
                CommentsCount = commentsCount,
                Manager = project.Manager == null ? null : new
                {
                    project.Manager.Id,
                    project.Manager.FirstName,
                    project.Manager.LastName,
                    project.Manager.Login,
                    project.Manager.Role
                }
            });
        }

        [HttpPost]
        public async Task<ActionResult<object>> PostProject(ProjectCreateUpdateDto dto)
        {
            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                ManagerId = dto.ManagerId,
                Status = dto.Status,
                Deadline = DateValueComparer.Normalize(dto.Deadline),
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            if (dto.ManagerId > 0)
            {
                var manager = await _context.Users.FindAsync(dto.ManagerId);
                if (manager != null)
                {
                    var projectUser = new ProjectUser
                    {
                        ProjectId = project.Id,
                        UserId = dto.ManagerId,
                        RoleInProject = "Project Manager",
                        JoinedAt = DateTime.UtcNow
                    };

                    _context.ProjectUsers.Add(projectUser);
                    await _context.SaveChangesAsync();
                }
            }

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
            project.Deadline = DateValueComparer.KeepIfEquivalent(project.Deadline, dto.Deadline);

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

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserProjects(int userId)
        {
            try
            {
                var userProjects = await _context.ProjectUsers
                    .Where(pu => pu.UserId == userId)
                    .Include(pu => pu.Project)
                    .ThenInclude(p => p.Manager)
                    .ToListAsync();

                if (!userProjects.Any())
                {
                    return Ok(new List<object>());
                }

                var projectIds = userProjects.Select(pu => pu.Project.Id).ToList();

                var participantsStats = await _context.ProjectUsers
                    .Where(pu => projectIds.Contains(pu.ProjectId))
                    .GroupBy(pu => pu.ProjectId)
                    .Select(g => new { ProjectId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var tasksStats = await _context.Tasks
                    .Where(t => t.ProjectId.HasValue && projectIds.Contains(t.ProjectId.Value))
                    .GroupBy(t => t.ProjectId)
                    .Select(g => new { ProjectId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var completedTasksStats = await _context.Tasks
                    .Where(t => t.ProjectId.HasValue && projectIds.Contains(t.ProjectId.Value) && t.Status == 3)
                    .GroupBy(t => t.ProjectId)
                    .Select(g => new { ProjectId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var taskIds = await _context.Tasks
                    .Where(t => t.ProjectId.HasValue && projectIds.Contains(t.ProjectId.Value))
                    .Select(t => t.Id)
                    .ToListAsync();

                var commentsStats = await _context.Comments
                    .Where(c => c.TaskId.HasValue && taskIds.Contains(c.TaskId.Value))
                    .GroupBy(c => _context.Tasks.FirstOrDefault(t => t.Id == c.TaskId.Value).ProjectId)
                    .Select(g => new { ProjectId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var result = new List<object>();
                foreach (var pu in userProjects)
                {
                    var project = pu.Project;
                    var projectId = project.Id;
                    
                    var participantsCount = participantsStats.FirstOrDefault(p => p.ProjectId == projectId)?.Count ?? 0;
                    var tasksCount = tasksStats.FirstOrDefault(t => t.ProjectId == projectId)?.Count ?? 0;
                    var completedTasks = completedTasksStats.FirstOrDefault(t => t.ProjectId == projectId)?.Count ?? 0;
                    var commentsCount = commentsStats.FirstOrDefault(c => c.ProjectId == projectId)?.Count ?? 0;
                    
                    var progress = tasksCount > 0 ? (double)completedTasks / tasksCount * 100 : 0;

                    result.Add(new
                    {
                        Id = project.Id,
                        Name = project.Name,
                        Description = project.Description,
                        ManagerId = project.ManagerId,
                        CreatedAt = project.CreatedAt,
                        Deadline = project.Deadline,
                        Status = project.Status,
                        Manager = project.Manager != null ? new
                        {
                            Id = project.Manager.Id,
                            FirstName = project.Manager.FirstName,
                            LastName = project.Manager.LastName,
                            Login = project.Manager.Login,
                            Role = project.Manager.Role
                        } : null,
                        RoleInProject = pu.RoleInProject,
                        JoinedAt = pu.JoinedAt,
                        ParticipantsCount = participantsCount,
                        TasksCount = tasksCount,
                        CommentsCount = commentsCount,
                        Progress = Math.Round(progress, 1)
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка получения проектов пользователя: {ex.Message}");
            }
        }

        [HttpGet("{id}/attachments")]
        public async Task<ActionResult<IEnumerable<TaskAttachmentDto>>> GetAttachments(int id)
        {
            if (!ProjectExists(id))
            {
                return NotFound();
            }

            var attachments = await _context.ProjectAttachments
                .Where(a => a.ProjectId == id)
                .OrderByDescending(a => a.UploadedAt)
                .Select(a => new TaskAttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    FileSize = a.FileSize,
                    UploadedAt = a.UploadedAt,
                    UploadedById = a.UploadedById,
                    UploadedByName = a.UploadedBy != null
                        ? $"{a.UploadedBy.FirstName} {a.UploadedBy.LastName}".Trim()
                        : null
                })
                .ToListAsync();

            return attachments;
        }

        [HttpPost("{id}/attachments")]
        [RequestSizeLimit(25_000_000)]
        public async Task<ActionResult<TaskAttachmentDto>> UploadAttachment(int id, IFormFile file, [FromQuery] int? uploadedById = null)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("Файл не выбран");
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            {
                return BadRequest("Разрешены только файлы Word, PDF, Excel и изображения");
            }

            var uploadsRoot = Path.Combine(_environment.ContentRootPath, "uploads", "projects", id.ToString());
            Directory.CreateDirectory(uploadsRoot);

            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadsRoot, storedFileName);

            await using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new ProjectAttachment
            {
                ProjectId = id,
                FileName = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                ContentType = file.ContentType ?? "application/octet-stream",
                FileSize = file.Length,
                UploadedAt = DateTime.UtcNow,
                UploadedById = uploadedById
            };

            _context.ProjectAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(DownloadAttachment), new { id, attachmentId = attachment.Id }, new TaskAttachmentDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                ContentType = attachment.ContentType,
                FileSize = attachment.FileSize,
                UploadedAt = attachment.UploadedAt,
                UploadedById = attachment.UploadedById
            });
        }

        [HttpGet("{id}/attachments/{attachmentId}")]
        public async Task<IActionResult> DownloadAttachment(int id, int attachmentId)
        {
            var attachment = await _context.ProjectAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.ProjectId == id);

            if (attachment == null)
            {
                return NotFound();
            }

            var fullPath = Path.Combine(_environment.ContentRootPath, "uploads", "projects", id.ToString(), attachment.StoredFileName);
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("Файл не найден на диске");
            }

            var contentType = string.IsNullOrWhiteSpace(attachment.ContentType)
                ? "application/octet-stream"
                : attachment.ContentType;

            return PhysicalFile(fullPath, contentType, attachment.FileName);
        }

        [HttpDelete("{id}/attachments/{attachmentId}")]
        public async Task<IActionResult> DeleteAttachment(int id, int attachmentId)
        {
            var attachment = await _context.ProjectAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.ProjectId == id);

            if (attachment == null)
            {
                return NotFound();
            }

            var fullPath = Path.Combine(_environment.ContentRootPath, "uploads", "projects", id.ToString(), attachment.StoredFileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            _context.ProjectAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var folder = Path.Combine(_environment.ContentRootPath, "uploads", "projects", id.ToString());
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
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