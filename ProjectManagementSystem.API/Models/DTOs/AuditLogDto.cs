namespace ProjectManagementSystem.API.Models.DTOs
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

    public class AuditLogFilterDto
    {
        public int? UserId { get; set; }
        public string? Action { get; set; }
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
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
}
