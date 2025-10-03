using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.API.Models.DTOs;
using ProjectManagementSystem.Database.Data;
using ProjectManagementSystem.Database.Entities;

namespace ProjectManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost("project/{projectId}")]
        public async Task<ActionResult<object>> CreateProjectComment(int projectId, [FromBody] CreateProjectCommentDto dto)
        {
            try
            {
                var firstTask = await _context.Tasks
                    .Where(t => t.ProjectId == projectId)
                    .FirstOrDefaultAsync();

                var comment = new Comment
                {
                    Content = dto.Content,
                    AuthorId = dto.AuthorId,
                    TaskId = firstTask?.Id, 
                    CreatedAt = DateTime.UtcNow
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                var author = await _context.Users.FindAsync(comment.AuthorId);
                var result = new
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                    Author = author != null ? new
                    {
                        Id = author.Id,
                        FirstName = author.FirstName,
                        LastName = author.LastName,
                        Email = author.Email
                    } : null
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка создания комментария: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetComment([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var comments = await _context.Comments
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new CommentResponseDto
                    {
                        Id = t.Id,
                        Content = t.Content,
                        TaskId = t.TaskId,
                        AuthorId = t.AuthorId,
                        CreatedAt = t.CreatedAt,
                        CommentUserResponceDto = new CommentUserResponceDto
                        {
                            Id = t.Author.Id,
                            FirstName = t.Author.FirstName,
                            LastName = t.Author.LastName,
                            Email = t.Author.Email,
                        },
                        CommentTaskReponseDto = new CommentTaskReponseDto
                        {
                            Id = t.Task.Id,
                            Title = t.Task.Title,
                            Description = t.Task.Description,
                        }
                    })
                    .ToListAsync();
                return comments;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки комментариев: {ex.Message}");
                return new List<CommentResponseDto>();

            }

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetComment(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.Task)
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            return comment;
        }

        [HttpGet("~/api/tasks/{taskId}/comments")]
        public async Task<ActionResult<IEnumerable<object>>> GetCommentsForTask(int taskId)
        {
            var comments = await _context.Comments
                .Where(c => c.TaskId == taskId)
                .Include(c => c.Author)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.TaskId,
                    c.AuthorId,
                    c.CreatedAt,
                    CommentUserResponceDto = c.Author == null ? null : new
                    {
                        c.Author.Id,
                        c.Author.FirstName,
                        c.Author.LastName,
                        c.Author.Email,
                        c.Author.Role
                    }
                })
                .ToListAsync();

            return Ok(comments);
        }

        [HttpPost("~/api/tasks/{taskId}/comments")]
        public async Task<ActionResult<object>> CreateCommentForTask(int taskId, [FromBody] CreateCommentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return NotFound("Задача не найдена");
            }

            var comment = new Comment
            {
                Content = dto.Content,
                TaskId = taskId,
                AuthorId = dto.AuthorId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var createdComment = await _context.Comments
                .Include(c => c.Author)
                .Where(c => c.Id == comment.Id)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.TaskId,
                    c.AuthorId,
                    c.CreatedAt,
                    Author = c.Author == null ? null : new
                    {
                        c.Author.Id,
                        c.Author.FirstName,
                        c.Author.LastName,
                        c.Author.Email,
                        c.Author.Role
                    }
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction("GetComment", new { id = comment.Id }, createdComment);
        }

        [HttpPost]
        public async Task<ActionResult<Comment>> PostComment(Comment comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            comment.CreatedAt = DateTime.UtcNow;

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var task = await _context.Tasks.FindAsync(comment.TaskId);
            if (task != null)
            {
                task.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction("GetComment", new { id = comment.Id }, comment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(int id, Comment comment)
        {
            if (id != comment.Id)
            {
                return BadRequest();
            }

            _context.Entry(comment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
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

        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<object>> GetProjectComments(int projectId, string? search = null, int page = 1, int pageSize = 10)
        {
            var baseQuery = _context.Comments
                .Where(c => c.TaskId.HasValue && _context.Tasks
                    .Where(t => t.ProjectId == projectId)
                    .Select(t => t.Id)
                    .Contains(c.TaskId.Value))
                .Include(c => c.Author);

            var query = baseQuery.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Content.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = comments.Select(c => new CommentResponseDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                TaskId = c.TaskId,
                AuthorId = c.AuthorId,
                CommentUserResponceDto = new CommentUserResponceDto
                {
                    Id = c.Author.Id,
                    FirstName = c.Author.FirstName,
                    LastName = c.Author.LastName,
                    Email = c.Author.Email
                },
                CommentTaskReponseDto = c.TaskId.HasValue ? new CommentTaskReponseDto
                {
                    Id = c.TaskId.Value,
                    Title = _context.Tasks.FirstOrDefault(t => t.Id == c.TaskId.Value)?.Title ?? "",
                    Description = _context.Tasks.FirstOrDefault(t => t.Id == c.TaskId.Value)?.Description ?? ""
                } : null
            }).ToList();

            return Ok(new
            {
                Comments = result,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
    }
}