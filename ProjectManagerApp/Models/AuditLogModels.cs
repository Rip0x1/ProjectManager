using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.WPF.Models
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string UserLogin { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string ActionText { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityTypeText { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    public class PagedAuditLogsDto
    {
        public List<AuditLogDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class ClearAuditLogsResultDto
    {
        public int DeletedCount { get; set; }
    }

    public class AuditLogItem
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string UserLogin { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string ActionText { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityTypeText { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Details { get; set; } = string.Empty;

        public string TimestampText => Timestamp.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss");
        public string UserDisplay => string.IsNullOrWhiteSpace(UserName)
            ? (string.IsNullOrWhiteSpace(UserLogin) ? "Система" : UserLogin)
            : UserName;
        public string EntityIdText => EntityId?.ToString() ?? "—";
        public string ActionColor => Action switch
        {
            "Create" => "#2196F3",
            "Update" => "#FF9800",
            "Delete" => "#F44336",
            "Login" => "#4CAF50",
            "Logout" => "#607D8B",
            "Register" => "#009688",
            "LoginFailed" => "#B71C1C",
            "Clear" => "#9C27B0",
            _ => "#757575"
        };
    }
}
