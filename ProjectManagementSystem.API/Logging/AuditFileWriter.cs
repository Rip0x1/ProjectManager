using System.Text;
using ProjectManagementSystem.API.Services;
using ProjectManagementSystem.Database.Entities;

namespace ProjectManagementSystem.API.Logging
{
    public sealed class AuditFileWriter : IDisposable
    {
        private static readonly TimeZoneInfo DisplayZone = ResolveDisplayZone();
        private readonly string _directory;
        private readonly object _sync = new();
        private StreamWriter? _writer;
        private string _currentDate = string.Empty;

        public AuditFileWriter(string directory)
        {
            _directory = directory;
            Directory.CreateDirectory(_directory);
        }

        public void Write(AuditLog log)
        {
            if (log == null)
            {
                return;
            }

            try
            {
                lock (_sync)
                {
                    EnsureWriter();
                    _writer!.WriteLine(Format(log));
                }
            }
            catch
            {
            }
        }

        private void EnsureWriter()
        {
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            if (_writer != null && _currentDate == date)
            {
                return;
            }

            _writer?.Dispose();
            _currentDate = date;
            var path = Path.Combine(_directory, $"audit-{date}.txt");
            var isNew = !File.Exists(path) || new FileInfo(path).Length == 0;
            var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            _writer = new StreamWriter(stream, new UTF8Encoding(true))
            {
                AutoFlush = true
            };

            if (isNew)
            {
                _writer.WriteLine("Журнал аудита системы управления проектами");
                _writer.WriteLine("Дата: " + DateTime.Now.ToString("dd.MM.yyyy"));
                _writer.WriteLine("Записи совпадают с журналом в приложении (раздел «Логи» у администратора).");
                _writer.WriteLine();
            }
        }

        private static string Format(AuditLog log)
        {
            var when = FormatWhen(log.Timestamp);
            var who = string.IsNullOrWhiteSpace(log.UserName)
                ? (string.IsNullOrWhiteSpace(log.UserLogin) ? "Система" : log.UserLogin)
                : log.UserName.Trim();
            var login = string.IsNullOrWhiteSpace(log.UserLogin) ? "—" : log.UserLogin;
            var action = AuditService.MapAction(log.Action, log.EntityType);
            var section = AuditService.MapEntityType(log.EntityType);
            var name = string.IsNullOrWhiteSpace(log.EntityName) ? "—" : log.EntityName.Trim();
            var entityId = log.EntityId?.ToString() ?? "—";
            var details = string.IsNullOrWhiteSpace(log.Details) ? "—" : log.Details.Trim();

            var builder = new StringBuilder();
            builder.AppendLine("--------------------------------------------------------------------------------");
            builder.AppendLine("Время:         " + when);
            builder.AppendLine("Кто:           " + who);
            builder.AppendLine("Логин:         " + login);
            builder.AppendLine("Действие:      " + action);
            builder.AppendLine("Раздел:        " + section);
            builder.AppendLine("Название:      " + name);
            builder.AppendLine("ID объекта:    " + entityId);
            builder.AppendLine("Что произошло: " + details);
            builder.Append("--------------------------------------------------------------------------------");
            return builder.ToString();
        }

        private static string FormatWhen(DateTime timestamp)
        {
            var utc = timestamp.Kind switch
            {
                DateTimeKind.Utc => timestamp,
                DateTimeKind.Local => timestamp.ToUniversalTime(),
                _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
            };

            return TimeZoneInfo.ConvertTimeFromUtc(utc, DisplayZone).ToString("dd.MM.yyyy HH:mm:ss");
        }

        private static TimeZoneInfo ResolveDisplayZone()
        {
            foreach (var id in new[] { "Europe/Moscow", "Russian Standard Time" })
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            return TimeZoneInfo.Local;
        }

        public void Dispose()
        {
            lock (_sync)
            {
                _writer?.Dispose();
                _writer = null;
            }
        }
    }
}
