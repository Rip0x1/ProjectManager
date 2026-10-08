using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProjectManagementSystem.API.Utilities;
using ProjectManagementSystem.Database.Entities;
using TaskEntity = ProjectManagementSystem.Database.Entities.Task;
using Task = System.Threading.Tasks.Task;

namespace ProjectManagementSystem.API.Services
{
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private static readonly HashSet<string> SupplementalTypes = new(StringComparer.Ordinal)
        {
            nameof(TaskAssignee),
            nameof(TaskAttachment),
            nameof(ProjectAttachment),
            nameof(CommentAttachment),
            nameof(ProjectUser)
        };

        private readonly ICurrentRequestUser _currentUser;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly List<(AuditLog Log, object Entity)> _addedLogs = new();
        private bool _isPatchingIds;

        public AuditSaveChangesInterceptor(ICurrentRequestUser currentUser, IHttpContextAccessor httpContextAccessor)
        {
            _currentUser = currentUser;
            _httpContextAccessor = httpContextAccessor;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            WriteAudit(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            WriteAudit(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            if (!_isPatchingIds)
            {
                PatchAddedEntityIds(eventData.Context);
            }

            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (!_isPatchingIds)
            {
                await PatchAddedEntityIdsAsync(eventData.Context, cancellationToken);
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private void WriteAudit(DbContext? context)
        {
            if (context == null || _currentUser.SuppressAudit || _isPatchingIds)
            {
                return;
            }

            _addedLogs.Clear();

            var http = _httpContextAccessor.HttpContext;
            var ip = http?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            var userAgent = Truncate(http?.Request.Headers.UserAgent.ToString(), 512);
            var entries = context.ChangeTracker.Entries()
                .Where(e => e.Entity is not AuditLog &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .ToList();

            ExcludeCancelledJoins(entries, nameof(TaskAssignee), "TaskId", "UserId");
            ExcludeCancelledJoins(entries, nameof(ProjectUser), "ProjectId", "UserId");

            var roots = entries.Where(e => !SupplementalTypes.Contains(e.Metadata.ClrType.Name)).ToList();
            var supplements = entries.Where(e => SupplementalTypes.Contains(e.Metadata.ClrType.Name)).ToList();
            var consumed = new HashSet<EntityEntry>();

            foreach (var root in roots)
            {
                if (consumed.Contains(root))
                {
                    continue;
                }

                if (root.State == EntityState.Modified && !HasMeaningfulPropertyChange(root))
                {
                    continue;
                }

                var related = supplements
                    .Where(s => !consumed.Contains(s) && BelongsTo(s, root))
                    .ToList();

                if (root.State == EntityState.Deleted)
                {
                    foreach (var cascaded in roots.Where(r => r != root && !consumed.Contains(r) && BelongsTo(r, root)))
                    {
                        related.Add(cascaded);
                        consumed.Add(cascaded);
                    }
                }

                foreach (var item in related)
                {
                    consumed.Add(item);
                }

                AddLog(context, root, related, ip, userAgent);
            }

            foreach (var leftover in supplements.Where(s => !consumed.Contains(s)))
            {
                if (leftover.State == EntityState.Modified && !HasMeaningfulPropertyChange(leftover))
                {
                    continue;
                }

                AddLog(context, leftover, Array.Empty<EntityEntry>(), ip, userAgent);
            }
        }

        private void AddLog(
            DbContext context,
            EntityEntry entry,
            IReadOnlyCollection<EntityEntry> related,
            string ip,
            string userAgent)
        {
            var action = entry.State switch
            {
                EntityState.Added => "Create",
                EntityState.Modified => "Update",
                EntityState.Deleted => "Delete",
                _ => entry.State.ToString()
            };

            var entityType = entry.Metadata.ClrType.Name;
            var details = BuildDetails(context, entry, related);
            if (string.IsNullOrWhiteSpace(details))
            {
                return;
            }

            var log = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = GetEntityId(entry) ?? GetRelatedParentId(entry),
                EntityName = Truncate(GetEntityName(entry.Entity), 256),
                UserId = _currentUser.UserId,
                UserLogin = _currentUser.Login,
                UserName = _currentUser.FullName,
                OldValues = null,
                NewValues = null,
                IpAddress = ip,
                UserAgent = userAgent,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            context.Set<AuditLog>().Add(log);

            if (entry.State == EntityState.Added)
            {
                _addedLogs.Add((log, entry.Entity));
            }
        }

        private void PatchAddedEntityIds(DbContext? context)
        {
            if (context == null || _addedLogs.Count == 0)
            {
                return;
            }

            ApplyGeneratedIds();
            _isPatchingIds = true;
            try
            {
                context.SaveChanges();
            }
            finally
            {
                _isPatchingIds = false;
                _addedLogs.Clear();
            }
        }

        private async Task PatchAddedEntityIdsAsync(DbContext? context, CancellationToken cancellationToken)
        {
            if (context == null || _addedLogs.Count == 0)
            {
                return;
            }

            ApplyGeneratedIds();
            _isPatchingIds = true;
            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                _isPatchingIds = false;
                _addedLogs.Clear();
            }
        }

        private void ApplyGeneratedIds()
        {
            foreach (var (log, entity) in _addedLogs)
            {
                var idProperty = entity.GetType().GetProperty("Id");
                if (idProperty?.GetValue(entity) is int id && id > 0)
                {
                    log.EntityId = id;
                }
            }
        }

        private static void ExcludeCancelledJoins(List<EntityEntry> entries, string typeName, string firstKey, string secondKey)
        {
            var added = entries.Where(e => e.Metadata.ClrType.Name == typeName && e.State == EntityState.Added).ToList();
            var deleted = entries.Where(e => e.Metadata.ClrType.Name == typeName && e.State == EntityState.Deleted).ToList();

            foreach (var add in added)
            {
                var match = deleted.FirstOrDefault(d => SameJoin(add, d, firstKey, secondKey));
                if (match == null)
                {
                    continue;
                }

                entries.Remove(add);
                entries.Remove(match);
                deleted.Remove(match);
            }
        }

        private static bool SameJoin(EntityEntry left, EntityEntry right, string firstKey, string secondKey)
        {
            return Equals(GetPropertyValue(left, firstKey, left.State == EntityState.Deleted), GetPropertyValue(right, firstKey, right.State == EntityState.Deleted))
                && Equals(GetPropertyValue(left, secondKey, left.State == EntityState.Deleted), GetPropertyValue(right, secondKey, right.State == EntityState.Deleted));
        }

        private static object? GetPropertyValue(EntityEntry entry, string name, bool original)
        {
            var property = entry.Properties.FirstOrDefault(p => p.Metadata.Name == name);
            if (property == null)
            {
                return null;
            }

            return original ? property.OriginalValue : property.CurrentValue;
        }

        private static bool BelongsTo(EntityEntry child, EntityEntry parent)
        {
            var parentType = parent.Metadata.ClrType.Name;
            var parentId = GetEntityId(parent);
            if (parentId is null or <= 0 && parent.State != EntityState.Added)
            {
                return false;
            }

            return (parentType, child.Metadata.ClrType.Name) switch
            {
                ("Task", nameof(TaskAssignee)) => IdsMatch(child, "TaskId", parentId, parent),
                ("Task", nameof(TaskAttachment)) => IdsMatch(child, "TaskId", parentId, parent),
                ("Task", nameof(Comment)) => IdsMatch(child, "TaskId", parentId, parent),
                (nameof(Project), nameof(ProjectUser)) => IdsMatch(child, "ProjectId", parentId, parent),
                (nameof(Project), nameof(ProjectAttachment)) => IdsMatch(child, "ProjectId", parentId, parent),
                (nameof(Comment), nameof(CommentAttachment)) => IdsMatch(child, "CommentId", parentId, parent),
                (nameof(User), nameof(ProjectUser)) => IdsMatch(child, "UserId", parentId, parent),
                _ => false
            };
        }

        private static bool IdsMatch(EntityEntry child, string fkName, int? parentId, EntityEntry parent)
        {
            if (parent.State == EntityState.Added)
            {
                if (parent.Entity is TaskEntity task && child.Entity is TaskAssignee assignee)
                {
                    return task.Assignees.Contains(assignee);
                }

                if (parent.Entity is TaskEntity parentTask && child.Entity is TaskAttachment attachment)
                {
                    return parentTask.Attachments.Contains(attachment);
                }

                if (parent.Entity is Project project && child.Entity is ProjectUser member)
                {
                    return project.ProjectUsers.Contains(member);
                }

                if (parent.Entity is Project parentProject && child.Entity is ProjectAttachment projectFile)
                {
                    return parentProject.Attachments.Contains(projectFile);
                }

                if (parent.Entity is Comment comment && child.Entity is CommentAttachment commentFile)
                {
                    return comment.Attachments.Contains(commentFile);
                }
            }

            return Equals(GetPropertyValue(child, fkName, child.State == EntityState.Deleted), parentId);
        }

        private static bool HasMeaningfulPropertyChange(EntityEntry entry)
        {
            return entry.Properties.Any(p =>
                p.IsModified
                && p.Metadata.Name is not "UpdatedAt" and not "CreatedAt"
                && !DateValueComparer.AreEquivalentValues(p.OriginalValue, p.CurrentValue));
        }

        private static int? GetEntityId(EntityEntry entry)
        {
            var property = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
            if (property?.CurrentValue is int current && current > 0)
            {
                return current;
            }

            if (property?.OriginalValue is int original && original > 0)
            {
                return original;
            }

            return null;
        }

        private static int? GetRelatedParentId(EntityEntry entry)
        {
            return entry.Metadata.ClrType.Name switch
            {
                nameof(TaskAssignee) => AsInt(GetPropertyValue(entry, "TaskId", entry.State == EntityState.Deleted)),
                nameof(TaskAttachment) => AsInt(GetPropertyValue(entry, "TaskId", entry.State == EntityState.Deleted)),
                nameof(ProjectAttachment) => AsInt(GetPropertyValue(entry, "ProjectId", entry.State == EntityState.Deleted)),
                nameof(CommentAttachment) => AsInt(GetPropertyValue(entry, "CommentId", entry.State == EntityState.Deleted)),
                nameof(ProjectUser) => AsInt(GetPropertyValue(entry, "ProjectId", entry.State == EntityState.Deleted)),
                _ => null
            };
        }

        private static int? AsInt(object? value)
        {
            return value is int number ? number : null;
        }

        private static string GetEntityName(object entity)
        {
            return entity switch
            {
                Project project => project.Name ?? string.Empty,
                TaskEntity task => task.Title ?? string.Empty,
                User user => $"{user.FirstName} {user.LastName}".Trim(),
                Comment comment => Truncate(comment.Content, 80),
                ProjectUser member => $"Проект {member.ProjectId}, пользователь {member.UserId}",
                TaskAssignee assignee => $"Задача {assignee.TaskId}, пользователь {assignee.UserId}",
                TaskAttachment taskFile => taskFile.FileName ?? string.Empty,
                ProjectAttachment projectFile => projectFile.FileName ?? string.Empty,
                CommentAttachment commentFile => commentFile.FileName ?? string.Empty,
                _ => entity.GetType().Name
            };
        }

        private static string BuildDetails(DbContext context, EntityEntry entry, IReadOnlyCollection<EntityEntry> related)
        {
            var name = GetEntityName(entry.Entity);
            var extras = DescribeRelated(context, related).ToList();

            if (SupplementalTypes.Contains(entry.Metadata.ClrType.Name))
            {
                return DescribeSupplemental(context, entry, name);
            }

            if (entry.State == EntityState.Added)
            {
                return DescribeCreated(context, entry, name, extras);
            }

            if (entry.State == EntityState.Deleted)
            {
                return DescribeDeleted(entry, name);
            }

            var changes = DescribePropertyChanges(context, entry);
            changes.AddRange(extras);
            if (changes.Count == 0)
            {
                return string.Empty;
            }

            return $"{DescribeUpdatedHeader(entry, name)} {string.Join("; ", changes)}.";
        }

        private static string DescribeCreated(DbContext context, EntityEntry entry, string name, List<string> extras)
        {
            var facts = DescribeSnapshot(context, entry);
            facts.AddRange(extras);
            var suffix = facts.Count == 0 ? string.Empty : " " + string.Join(". ", facts) + ".";

            return entry.Metadata.ClrType.Name switch
            {
                "Task" => $"Создана задача «{OrDash(name)}».{suffix}",
                "Project" => $"Создан проект «{OrDash(name)}».{suffix}",
                "User" => $"Создан пользователь {OrDash(name)}.{suffix}",
                "Comment" => $"Добавлен комментарий: «{OrDash(name)}».{suffix}",
                _ => $"Создано: {AuditService.MapEntityType(entry.Metadata.ClrType.Name)} «{OrDash(name)}».{suffix}"
            };
        }

        private static string DescribeDeleted(EntityEntry entry, string name)
        {
            return entry.Metadata.ClrType.Name switch
            {
                "Task" => $"Удалена задача «{OrDash(name)}».",
                "Project" => $"Удалён проект «{OrDash(name)}».",
                "User" => $"Удалён пользователь {OrDash(name)}.",
                "Comment" => $"Удалён комментарий «{OrDash(name)}».",
                _ => $"Удалено: {AuditService.MapEntityType(entry.Metadata.ClrType.Name)} «{OrDash(name)}»."
            };
        }

        private static string DescribeUpdatedHeader(EntityEntry entry, string name)
        {
            return entry.Metadata.ClrType.Name switch
            {
                "Task" => $"Изменена задача «{OrDash(name)}»:",
                "Project" => $"Изменён проект «{OrDash(name)}»:",
                "User" => $"Изменён пользователь {OrDash(name)}:",
                "Comment" => $"Изменён комментарий:",
                _ => $"Изменено ({AuditService.MapEntityType(entry.Metadata.ClrType.Name)} «{OrDash(name)}»):"
            };
        }

        private static string DescribeSupplemental(DbContext context, EntityEntry entry, string name)
        {
            var fileName = string.IsNullOrWhiteSpace(name) ? "файл" : name;
            return entry.Metadata.ClrType.Name switch
            {
                nameof(TaskAttachment) =>
                    entry.State == EntityState.Deleted
                        ? $"У задачи удалён файл «{fileName}»."
                        : $"К задаче добавлен файл «{fileName}».",
                nameof(ProjectAttachment) =>
                    entry.State == EntityState.Deleted
                        ? $"У проекта удалён файл «{fileName}»."
                        : $"К проекту добавлен файл «{fileName}».",
                nameof(CommentAttachment) =>
                    entry.State == EntityState.Deleted
                        ? $"У комментария удалён файл «{fileName}»."
                        : $"К комментарию добавлен файл «{fileName}».",
                nameof(TaskAssignee) =>
                    entry.State == EntityState.Deleted
                        ? $"С задачи снят исполнитель {FormatUserRef(context, entry)}."
                        : $"На задачу назначен исполнитель {FormatUserRef(context, entry)}.",
                nameof(ProjectUser) =>
                    entry.State == EntityState.Deleted
                        ? $"Из проекта удалён участник {FormatUserRef(context, entry)}."
                        : $"В проект добавлен участник {FormatUserRef(context, entry)}.",
                _ => $"{AuditService.MapAction(entry.State == EntityState.Deleted ? "Delete" : "Create", entry.Metadata.ClrType.Name)}."
            };
        }

        private static IEnumerable<string> DescribeRelated(DbContext context, IReadOnlyCollection<EntityEntry> related)
        {
            foreach (var group in related.GroupBy(r => r.Metadata.ClrType.Name))
            {
                var added = group.Where(e => e.State == EntityState.Added).ToList();
                var deleted = group.Where(e => e.State == EntityState.Deleted).ToList();

                if (group.Key is nameof(TaskAttachment) or nameof(ProjectAttachment) or nameof(CommentAttachment))
                {
                    foreach (var item in added)
                    {
                        yield return $"добавлен файл «{GetEntityName(item.Entity)}»";
                    }

                    foreach (var item in deleted)
                    {
                        yield return $"удалён файл «{GetEntityName(item.Entity)}»";
                    }

                    continue;
                }

                if (group.Key == nameof(TaskAssignee))
                {
                    if (added.Count > 0)
                    {
                        yield return "назначены исполнители " + JoinNames(added.Select(e => FormatUserRef(context, e)));
                    }

                    if (deleted.Count > 0)
                    {
                        yield return "сняты исполнители " + JoinNames(deleted.Select(e => FormatUserRef(context, e)));
                    }

                    continue;
                }

                if (group.Key == nameof(ProjectUser))
                {
                    if (added.Count > 0)
                    {
                        yield return "добавлены участники " + JoinNames(added.Select(e => FormatUserRef(context, e)));
                    }

                    if (deleted.Count > 0)
                    {
                        yield return "удалены участники " + JoinNames(deleted.Select(e => FormatUserRef(context, e)));
                    }
                }
            }
        }

        private static string FormatUserRef(DbContext context, EntityEntry entry)
        {
            int? userId = entry.Entity switch
            {
                TaskAssignee assignee => assignee.UserId,
                ProjectUser member => member.UserId,
                _ => AsInt(GetPropertyValue(entry, "UserId", entry.State == EntityState.Deleted))
            };

            return ResolveUserName(context, userId);
        }

        private static List<string> DescribeSnapshot(DbContext context, EntityEntry entry)
        {
            var facts = new List<string>();
            var typeName = entry.Metadata.ClrType.Name;

            void Add(string label, string? value)
            {
                if (!string.IsNullOrWhiteSpace(value) && value != "не указано")
                {
                    facts.Add($"{label}: {value}");
                }
            }

            Add("Статус", FormatDisplayValue(context, typeName, "Status", GetCurrent(entry, "Status")));
            Add("Приоритет", FormatDisplayValue(context, typeName, "Priority", GetCurrent(entry, "Priority")));
            Add("Проект", FormatDisplayValue(context, typeName, "ProjectId", GetCurrent(entry, "ProjectId")));
            Add("Руководитель", FormatDisplayValue(context, typeName, "ManagerId", GetCurrent(entry, "ManagerId")));
            Add("Исполнитель", FormatDisplayValue(context, typeName, "AssigneeId", GetCurrent(entry, "AssigneeId")));
            Add("Роль", FormatDisplayValue(context, typeName, "Role", GetCurrent(entry, "Role")));
            Add("Логин", FormatDisplayValue(context, typeName, "Login", GetCurrent(entry, "Login")));
            Add("Срок", FormatDisplayValue(context, typeName, "DueDate", GetCurrent(entry, "DueDate")));
            Add("Дедлайн", FormatDisplayValue(context, typeName, "Deadline", GetCurrent(entry, "Deadline")));

            var description = FormatDisplayValue(context, typeName, "Description", GetCurrent(entry, "Description"));
            if (description != "не указано")
            {
                Add("Описание", description);
            }

            return facts;
        }

        private static object? GetCurrent(EntityEntry entry, string name)
        {
            return entry.Properties.FirstOrDefault(p => p.Metadata.Name == name)?.CurrentValue;
        }

        private static List<string> DescribePropertyChanges(DbContext context, EntityEntry entry)
        {
            var changes = new List<string>();
            var typeName = entry.Metadata.ClrType.Name;

            foreach (var property in entry.Properties)
            {
                if (!property.IsModified || property.Metadata.Name is "UpdatedAt" or "CreatedAt" or "StoredFileName")
                {
                    continue;
                }

                if (DateValueComparer.AreEquivalentValues(property.OriginalValue, property.CurrentValue))
                {
                    continue;
                }

                if (property.Metadata.Name == "PasswordHash")
                {
                    changes.Add("пароль обновлён");
                    continue;
                }

                var label = MapPropertyName(property.Metadata.Name);
                var from = FormatDisplayValue(context, typeName, property.Metadata.Name, property.OriginalValue);
                var to = FormatDisplayValue(context, typeName, property.Metadata.Name, property.CurrentValue);
                changes.Add($"{label}: «{from}» → «{to}»");
            }

            return changes;
        }

        private static string MapPropertyName(string name) => name switch
        {
            "Title" => "название",
            "Name" => "название",
            "Description" => "описание",
            "Status" => "статус",
            "Priority" => "приоритет",
            "ProjectId" => "проект",
            "AssigneeId" => "исполнитель",
            "AuthorId" => "автор",
            "ManagerId" => "руководитель",
            "DueDate" => "срок",
            "Deadline" => "дедлайн",
            "CompletedAt" => "дата завершения",
            "FirstName" => "имя",
            "LastName" => "фамилия",
            "Login" => "логин",
            "Role" => "роль",
            "Content" => "текст",
            _ => name
        };

        private static string FormatDisplayValue(DbContext context, string entityType, string propertyName, object? value)
        {
            if (value == null)
            {
                return "не указано";
            }

            if (propertyName == "Status" && value is int status)
            {
                if (entityType == "Project")
                {
                    return status switch
                    {
                        0 => "Активный",
                        1 => "Завершён",
                        2 => "Приостановлен",
                        _ => status.ToString()
                    };
                }

                return status switch
                {
                    0 => "Новая",
                    1 => "В работе",
                    2 => "На проверке",
                    3 => "Завершена",
                    4 => "Отложено",
                    5 => "Проблема",
                    _ => status.ToString()
                };
            }

            if (propertyName == "Priority" && value is int priority)
            {
                return priority switch
                {
                    0 => "Низкий",
                    1 => "Средний",
                    2 => "Высокий",
                    3 => "Критический",
                    _ => priority.ToString()
                };
            }

            if (propertyName == "Role" && value is int role)
            {
                return role switch
                {
                    0 => "Пользователь",
                    1 => "Менеджер",
                    2 => "Администратор",
                    _ => role.ToString()
                };
            }

            if (propertyName is "ProjectId" && value is int projectId)
            {
                return ResolveProjectName(context, projectId);
            }

            if (propertyName is "AssigneeId" or "AuthorId" or "ManagerId" or "UserId" && value is int userId)
            {
                return ResolveUserName(context, userId);
            }

            if (value is DateTime dateTime)
            {
                return DateValueComparer.CalendarDate(dateTime).ToString("dd.MM.yyyy");
            }

            var text = value.ToString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return "не указано";
            }

            return text.Length > 80 ? text[..80] + "…" : text;
        }

        private static string ResolveUserName(DbContext context, int? userId)
        {
            if (userId is not int id || id <= 0)
            {
                return "не указан";
            }

            var user = context.Set<User>().Local.FirstOrDefault(u => u.Id == id)
                       ?? context.ChangeTracker.Entries<User>().FirstOrDefault(e => e.Entity.Id == id)?.Entity
                       ?? context.Set<User>().Find(id);

            if (user == null)
            {
                return $"пользователь №{id}";
            }

            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            return string.IsNullOrWhiteSpace(fullName) ? user.Login : fullName;
        }

        private static string ResolveProjectName(DbContext context, int projectId)
        {
            var project = context.Set<Project>().Local.FirstOrDefault(p => p.Id == projectId)
                          ?? context.ChangeTracker.Entries<Project>().FirstOrDefault(e => e.Entity.Id == projectId)?.Entity
                          ?? context.Set<Project>().Find(projectId);

            return project == null || string.IsNullOrWhiteSpace(project.Name)
                ? $"проект №{projectId}"
                : $"«{project.Name}»";
        }

        private static string JoinNames(IEnumerable<string> names)
        {
            var list = names.Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
            if (list.Count == 0)
            {
                return "—";
            }

            if (list.Count == 1)
            {
                return list[0];
            }

            return string.Join(", ", list.Take(list.Count - 1)) + " и " + list[^1];
        }

        private static string OrDash(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "без названия" : value;
        }

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
