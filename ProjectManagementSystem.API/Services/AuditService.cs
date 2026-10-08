using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.API.Models.DTOs;
using ProjectManagementSystem.Database.Data;
using ProjectManagementSystem.Database.Entities;
using Task = System.Threading.Tasks.Task;

namespace ProjectManagementSystem.API.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAsync(string action, string entityType, int? entityId, string entityName,
                                   int? userId, string userLogin, string userName, string? oldValues, string? newValues,
                                   string ipAddress, string userAgent, string details = "")
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    EntityName = entityName ?? string.Empty,
                    UserId = userId,
                    UserLogin = userLogin ?? string.Empty,
                    UserName = userName ?? string.Empty,
                    OldValues = oldValues,
                    NewValues = newValues,
                    IpAddress = ipAddress ?? string.Empty,
                    UserAgent = Truncate(userAgent, 512),
                    Details = details ?? string.Empty,
                    Timestamp = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка записи аудита");
            }
        }

        public async Task<PagedAuditLogsDto> GetLogsAsync(AuditLogFilterDto filter)
        {
            filter.PageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            filter.PageSize = filter.PageSize < 1 ? 25 : Math.Min(filter.PageSize, 100);

            var query = ApplyFilter(_context.AuditLogs.AsNoTracking(), filter);
            var totalCount = await query.CountAsync();
            var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var items = await query
                .OrderByDescending(a => a.Timestamp)
                .ThenByDescending(a => a.Id)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserLogin = a.UserLogin,
                    UserName = a.UserName,
                    Action = a.Action,
                    ActionText = MapAction(a.Action, a.EntityType),
                    EntityType = a.EntityType,
                    EntityTypeText = MapEntityType(a.EntityType),
                    EntityId = a.EntityId,
                    EntityName = a.EntityName,
                    IpAddress = a.IpAddress,
                    UserAgent = a.UserAgent,
                    Timestamp = a.Timestamp,
                    Details = a.Details
                })
                .ToListAsync();

            return new PagedAuditLogsDto
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<int> DeleteLogsAsync(AuditLogFilterDto filter)
        {
            return await ApplyFilter(_context.AuditLogs, filter).ExecuteDeleteAsync();
        }

        public static string DescribeFilter(AuditLogFilterDto filter)
        {
            var parts = new List<string>();

            if (filter.FromDate.HasValue)
            {
                parts.Add($"с {filter.FromDate.Value:dd.MM.yyyy}");
            }

            if (filter.ToDate.HasValue)
            {
                parts.Add($"по {filter.ToDate.Value:dd.MM.yyyy}");
            }

            if (!string.IsNullOrWhiteSpace(filter.Action))
            {
                parts.Add("действие «" + MapAction(filter.Action, filter.EntityType) + "»");
            }

            if (!string.IsNullOrWhiteSpace(filter.EntityType))
            {
                parts.Add("объект «" + MapEntityType(filter.EntityType) + "»");
            }

            if (filter.UserId.HasValue)
            {
                parts.Add($"пользователь №{filter.UserId.Value}");
            }

            if (filter.EntityId.HasValue)
            {
                parts.Add($"ID объекта {filter.EntityId.Value}");
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                parts.Add($"поиск «{filter.SearchTerm.Trim()}»");
            }

            return parts.Count == 0 ? "все записи" : string.Join(", ", parts);
        }

        private static IQueryable<AuditLog> ApplyFilter(IQueryable<AuditLog> query, AuditLogFilterDto filter)
        {
            if (filter.UserId.HasValue)
            {
                query = query.Where(a => a.UserId == filter.UserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Action))
            {
                query = query.Where(a => a.Action == filter.Action);
            }

            if (!string.IsNullOrWhiteSpace(filter.EntityType))
            {
                query = query.Where(a => a.EntityType == filter.EntityType);
            }

            if (filter.EntityId.HasValue)
            {
                query = query.Where(a => a.EntityId == filter.EntityId.Value);
            }

            if (filter.FromDate.HasValue)
            {
                var from = filter.FromDate.Value.Date;
                query = query.Where(a => a.Timestamp >= from);
            }

            if (filter.ToDate.HasValue)
            {
                var to = filter.ToDate.Value.Date.AddDays(1);
                query = query.Where(a => a.Timestamp < to);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(a =>
                    a.Id.ToString().Contains(term) ||
                    (a.EntityId.HasValue && a.EntityId.Value.ToString().Contains(term)) ||
                    (a.UserId.HasValue && a.UserId.Value.ToString().Contains(term)) ||
                    a.UserLogin.Contains(term) ||
                    a.UserName.Contains(term) ||
                    a.EntityName.Contains(term) ||
                    a.Action.Contains(term) ||
                    a.EntityType.Contains(term) ||
                    a.Details.Contains(term) ||
                    a.IpAddress.Contains(term));
            }

            return query;
        }

        public static string MapAction(string action, string? entityType = null) => (action, entityType) switch
        {
            ("Create", "TaskAttachment") => "Добавление вложения",
            ("Create", "ProjectAttachment") => "Добавление вложения",
            ("Create", "CommentAttachment") => "Добавление вложения",
            ("Delete", "TaskAttachment") => "Удаление вложения",
            ("Delete", "ProjectAttachment") => "Удаление вложения",
            ("Delete", "CommentAttachment") => "Удаление вложения",
            ("Create", "TaskAssignee") => "Назначение исполнителя",
            ("Delete", "TaskAssignee") => "Снятие исполнителя",
            ("Create", "ProjectUser") => "Добавление участника",
            ("Delete", "ProjectUser") => "Удаление участника",
            ("Create", _) => "Создание",
            ("Update", _) => "Изменение",
            ("Delete", _) => "Удаление",
            ("Login", _) => "Вход",
            ("Logout", _) => "Выход",
            ("Register", _) => "Регистрация",
            ("LoginFailed", _) => "Неудачный вход",
            ("Clear", _) => "Очистка журнала",
            _ => action
        };

        public static string MapEntityType(string entityType) => entityType switch
        {
            "Project" => "Проект",
            "Task" => "Задача",
            "User" => "Пользователь",
            "Comment" => "Комментарий",
            "ProjectUser" => "Участник проекта",
            "TaskAssignee" => "Исполнитель",
            "TaskAttachment" => "Вложение задачи",
            "ProjectAttachment" => "Вложение проекта",
            "CommentAttachment" => "Вложение комментария",
            "AuditLog" => "Журнал",
            _ => entityType
        };

        private static string Truncate(string? value, int max)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Length <= max ? value : value[..max];
        }
    }
}
