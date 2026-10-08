using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Database.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }

        [MaxLength(256)]
        public string UserLogin { get; set; } = string.Empty;

        [MaxLength(256)]
        public string UserName { get; set; } = string.Empty;

        [MaxLength(64)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(64)]
        public string EntityType { get; set; } = string.Empty;

        public int? EntityId { get; set; }

        [MaxLength(256)]
        public string EntityName { get; set; } = string.Empty;

        public string? OldValues { get; set; }
        public string? NewValues { get; set; }

        [MaxLength(64)]
        public string IpAddress { get; set; } = string.Empty;

        [MaxLength(512)]
        public string UserAgent { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        public string Details { get; set; } = string.Empty;

        public User? User { get; set; }
    }
}
