using System.Text;
using ProjectManagementSystem.API.Services;
using ProjectManagementSystem.Database.Entities;

namespace ProjectManagementSystem.API.Logging
{
    public sealed class AuditFileWriter : IDisposable
    {
        private static readonly TimeZoneInfo DisplayZone = ResolveDisplayZone();
        private readonly string _filePath;
        private readonly object _sync = new();
        private StreamWriter? _writer;

        public AuditFileWriter(string directory)
        {
            Directory.CreateDirectory(directory);
            _filePath = Path.Combine(directory, "logs.txt");
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
            if (_writer != null)
            {
                return;
            }

            var stream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            _writer = new StreamWriter(stream, new UTF8Encoding(true))
            {
                AutoFlush = true
            };
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

            return $"Time: {when} | Who: {who} | Login: {login} | Action: {action} | Section: {section} | Name: {name} | ID entity: {entityId} | Details: {details}";
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
