using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.API.Models.DTOs;
using ProjectManagementSystem.API.Utilities;
using ProjectManagementSystem.Database.Data;
using ProjectManagementSystem.Database.Entities;
using Task = ProjectManagementSystem.Database.Entities.Task;

namespace ProjectManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".doc", ".docx", ".pdf", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
        };

        public TasksController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetTasks(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null,
            [FromQuery] int? projectId = null,
            [FromQuery] int? assigneeId = null,
            [FromQuery] DateTime? dueFrom = null,
            [FromQuery] DateTime? dueTo = null)
        {
            try
            {
                var query = _context.Tasks.AsNoTracking();

                if (projectId.HasValue)
                {
                    query = query.Where(t => t.ProjectId == projectId.Value);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(t =>
                        t.Id.ToString().Contains(search) ||
                        (t.ProjectId.HasValue && t.ProjectId.Value.ToString().Contains(search)) ||
                        t.Title.Contains(search) ||
                        t.Description.Contains(search));
                }

                if (assigneeId.HasValue)
                {
                    query = query.Where(t =>
                        t.AssigneeId == assigneeId.Value ||
                        t.Assignees.Any(a => a.UserId == assigneeId.Value));
                }

                if (dueFrom.HasValue)
                {
                    var from = dueFrom.Value.Date;
                    query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value >= from);
                }

                if (dueTo.HasValue)
                {
                    var to = dueTo.Value.Date.AddDays(1);
                    query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value < to);
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var rows = await query
                    .OrderByDescending(t => t.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        t.Id,
                        t.Title,
                        t.Description,
                        t.Status,
                        t.Priority,
                        t.ProjectId,
                        ProjectName = t.Project != null ? t.Project.Name : null,
                        t.AuthorId,
                        AuthorFirstName = t.Author != null ? t.Author.FirstName : null,
                        AuthorLastName = t.Author != null ? t.Author.LastName : null,
                        AuthorLogin = t.Author != null ? t.Author.Login : null,
                        t.AssigneeId,
                        AssigneeFirstName = t.Assignee != null ? t.Assignee.FirstName : null,
                        AssigneeLastName = t.Assignee != null ? t.Assignee.LastName : null,
                        Assignees = t.Assignees.Select(a => new
                        {
                            a.UserId,
                            FirstName = a.User != null ? a.User.FirstName : null,
                            LastName = a.User != null ? a.User.LastName : null,
                            Login = a.User != null ? a.User.Login : null
                        }),
                        t.CreatedAt,
                        t.UpdatedAt,
                        t.DueDate,
                        t.CompletedAt,
                        CommentsCount = t.Comments.Count,
                        AttachmentsCount = t.Attachments.Count
                    })
                    .ToListAsync();

                var tasks = rows.Select(t =>
                {
                    var assignees = t.Assignees
                        .Select(a => new TaskAssigneeDto
                        {
                            UserId = a.UserId,
                            Name = FormatPersonName(a.FirstName, a.LastName, a.Login, a.UserId)
                        })
                        .ToList();

                    if (assignees.Count == 0 && t.AssigneeId.HasValue)
                    {
                        assignees.Add(new TaskAssigneeDto
                        {
                            UserId = t.AssigneeId.Value,
                            Name = FormatPersonName(t.AssigneeFirstName, t.AssigneeLastName, null, t.AssigneeId.Value)
                        });
                    }

                    var assigneeName = assignees.Count > 0
                        ? string.Join(", ", assignees.Select(a => a.Name))
                        : null;

                    return new TaskResponseDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        Status = t.Status,
                        Priority = t.Priority,
                        ProjectId = t.ProjectId,
                        ProjectName = string.IsNullOrWhiteSpace(t.ProjectName) ? "Без проекта" : t.ProjectName,
                        AuthorId = t.AuthorId,
                        AuthorName = FormatPersonName(t.AuthorFirstName, t.AuthorLastName, t.AuthorLogin, t.AuthorId),
                        AssigneeId = t.AssigneeId ?? assignees.FirstOrDefault()?.UserId,
                        AssigneeName = assigneeName,
                        AssigneeIds = assignees.Select(a => a.UserId).ToList(),
                        Assignees = assignees,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        DueDate = t.DueDate,
                        CompletedAt = t.CompletedAt,
                        CommentsCount = t.CommentsCount,
                        AttachmentsCount = t.AttachmentsCount,
                        Attachments = new List<TaskAttachmentDto>()
                    };
                }).ToList();

                return Ok(new
                {
                    Tasks = tasks,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки задач: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDto>> GetTask(int id)
        {
            var entity = await _context.Tasks
                .AsNoTracking()
                .Include(t => t.Project)
                .Include(t => t.Author)
                .Include(t => t.Assignee)
                .Include(t => t.Assignees)
                    .ThenInclude(a => a.User)
                .Include(t => t.Attachments)
                    .ThenInclude(a => a.UploadedBy)
                .Include(t => t.Comments)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
            {
                return NotFound();
            }

            return MapTask(entity, includeAttachments: true);
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Task>>> GetTasksByStatus(int status)
        {
            return await _context.Tasks
                .Where(t => t.Status == status)
                .Include(t => t.Project)
                .Include(t => t.Assignee)
                .Include(t => t.Assignees)
                .ToListAsync();
        }

        [HttpGet("priority/{priority}")]
        public async Task<ActionResult<IEnumerable<Task>>> GetTasksByPriority(int priority)
        {
            return await _context.Tasks
                .Where(t => t.Priority == priority)
                .Include(t => t.Project)
                .Include(t => t.Assignee)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<object>> PostTask(TaskCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var task = new Task
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                Priority = dto.Priority,
                ProjectId = dto.ProjectId is > 0 ? dto.ProjectId : null,
                AuthorId = dto.AuthorId,
                DueDate = DateValueComparer.Normalize(dto.DueDate),
                CompletedAt = DateValueComparer.Normalize(dto.CompletedAt),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            ApplyAssignees(task, dto);

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTask", new { id = task.Id }, new { task.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTask(int id, TaskCreateUpdateDto dto)
        {
            var task = await _context.Tasks
                .Include(t => t.Assignees)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Status = dto.Status;
            task.Priority = dto.Priority;
            task.ProjectId = dto.ProjectId is > 0 ? dto.ProjectId : null;
            task.DueDate = DateValueComparer.KeepIfEquivalent(task.DueDate, dto.DueDate);
            task.CompletedAt = DateValueComparer.KeepIfEquivalent(task.CompletedAt, dto.CompletedAt);
            task.UpdatedAt = DateTime.UtcNow;
            ApplyAssignees(task, dto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
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

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] int status)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.Status = status;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}/assignee")]
        public async Task<IActionResult> UpdateTaskAssignee(int id, [FromBody] int assigneeId)
        {
            var task = await _context.Tasks
                .Include(t => t.Assignees)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            task.AssigneeId = assigneeId;
            task.UpdatedAt = DateTime.UtcNow;
            task.Assignees.Clear();
            task.Assignees.Add(new TaskAssignee { TaskId = id, UserId = assigneeId });

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/attachments")]
        public async Task<ActionResult<IEnumerable<TaskAttachmentDto>>> GetAttachments(int id)
        {
            if (!TaskExists(id))
            {
                return NotFound();
            }

            var attachments = await _context.TaskAttachments
                .Where(a => a.TaskId == id)
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
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
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

            var uploadsRoot = Path.Combine(_environment.ContentRootPath, "uploads", "tasks", id.ToString());
            Directory.CreateDirectory(uploadsRoot);

            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadsRoot, storedFileName);

            await using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new TaskAttachment
            {
                TaskId = id,
                FileName = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                ContentType = file.ContentType ?? "application/octet-stream",
                FileSize = file.Length,
                UploadedAt = DateTime.UtcNow,
                UploadedById = uploadedById
            };

            _context.TaskAttachments.Add(attachment);
            task.UpdatedAt = DateTime.UtcNow;
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
            var attachment = await _context.TaskAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.TaskId == id);

            if (attachment == null)
            {
                return NotFound();
            }

            var fullPath = Path.Combine(_environment.ContentRootPath, "uploads", "tasks", id.ToString(), attachment.StoredFileName);
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
            var attachment = await _context.TaskAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.TaskId == id);

            if (attachment == null)
            {
                return NotFound();
            }

            var fullPath = Path.Combine(_environment.ContentRootPath, "uploads", "tasks", id.ToString(), attachment.StoredFileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            _context.TaskAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            var folder = Path.Combine(_environment.ContentRootPath, "uploads", "tasks", id.ToString());
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static void ApplyAssignees(Task task, TaskCreateUpdateDto dto)
        {
            var ids = (dto.AssigneeIds ?? new List<int>())
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (ids.Count == 0 && dto.AssigneeId.HasValue && dto.AssigneeId.Value > 0)
            {
                ids.Add(dto.AssigneeId.Value);
            }

            task.AssigneeId = ids.Count > 0 ? ids[0] : null;
            task.Assignees ??= new List<TaskAssignee>();

            var desired = ids.ToHashSet();
            foreach (var assignee in task.Assignees.Where(a => !desired.Contains(a.UserId)).ToList())
            {
                task.Assignees.Remove(assignee);
            }

            var current = task.Assignees.Select(a => a.UserId).ToHashSet();
            foreach (var userId in ids.Where(id => !current.Contains(id)))
            {
                task.Assignees.Add(new TaskAssignee { UserId = userId });
            }
        }

        private static string FormatPersonName(string? firstName, string? lastName, string? login, int userId)
        {
            var fullName = $"{firstName} {lastName}".Trim();
            if (!string.IsNullOrEmpty(fullName))
            {
                return fullName;
            }

            return string.IsNullOrEmpty(login) ? $"Пользователь #{userId}" : login;
        }

        private static TaskResponseDto MapTask(Task t, bool includeAttachments = false)
        {
            var assignees = t.Assignees
                .Select(a => new TaskAssigneeDto
                {
                    UserId = a.UserId,
                    Name = a.User != null
                        ? (!string.IsNullOrEmpty(a.User.FirstName) || !string.IsNullOrEmpty(a.User.LastName)
                            ? $"{a.User.FirstName} {a.User.LastName}".Trim()
                            : a.User.Login ?? $"Пользователь #{a.UserId}")
                        : $"Пользователь #{a.UserId}"
                })
                .ToList();

            if (assignees.Count == 0 && t.Assignee != null)
            {
                assignees.Add(new TaskAssigneeDto
                {
                    UserId = t.Assignee.Id,
                    Name = $"{t.Assignee.FirstName} {t.Assignee.LastName}".Trim()
                });
            }

            var assigneeName = assignees.Count > 0
                ? string.Join(", ", assignees.Select(a => a.Name))
                : (t.Assignee != null ? $"{t.Assignee.FirstName} {t.Assignee.LastName}".Trim() : null);

            return new TaskResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                ProjectId = t.ProjectId,
                ProjectName = t.Project != null ? t.Project.Name : "Без проекта",
                AuthorId = t.AuthorId,
                AuthorName = t.Author != null ?
                    (!string.IsNullOrEmpty(t.Author.FirstName) || !string.IsNullOrEmpty(t.Author.LastName)
                        ? $"{t.Author.FirstName} {t.Author.LastName}".Trim()
                        : t.Author.Login ?? $"Пользователь #{t.AuthorId}")
                    : $"Пользователь #{t.AuthorId}",
                AssigneeId = t.AssigneeId ?? assignees.FirstOrDefault()?.UserId,
                AssigneeName = assigneeName,
                AssigneeIds = assignees.Select(a => a.UserId).ToList(),
                Assignees = assignees,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                DueDate = t.DueDate,
                CompletedAt = t.CompletedAt,
                CommentsCount = t.Comments.Count,
                AttachmentsCount = t.Attachments.Count,
                Attachments = includeAttachments
                    ? t.Attachments.Select(a => new TaskAttachmentDto
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
                    }).ToList()
                    : new List<TaskAttachmentDto>()
            };
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
