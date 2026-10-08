using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.API.Models.DTOs;
using ProjectManagementSystem.API.Services;

namespace ProjectManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditService _auditService;
        private readonly ICurrentRequestUser _currentUser;

        public AuditLogsController(IAuditService auditService, ICurrentRequestUser currentUser)
        {
            _auditService = auditService;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<ActionResult<PagedAuditLogsDto>> GetLogs(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? action = null,
            [FromQuery] string? entityType = null,
            [FromQuery] int? userId = null,
            [FromQuery] int? entityId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            if (!_currentUser.IsAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = "Доступ только для администратора" });
            }

            var result = await _auditService.GetLogsAsync(new AuditLogFilterDto
            {
                SearchTerm = searchTerm,
                Action = action,
                EntityType = entityType,
                UserId = userId,
                EntityId = entityId,
                FromDate = fromDate,
                ToDate = toDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            });

            return Ok(result);
        }

        [HttpDelete]
        public async Task<ActionResult<ClearAuditLogsResultDto>> ClearLogs(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? action = null,
            [FromQuery] string? entityType = null,
            [FromQuery] int? userId = null,
            [FromQuery] int? entityId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            if (!_currentUser.IsAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = "Доступ только для администратора" });
            }

            var filter = new AuditLogFilterDto
            {
                SearchTerm = searchTerm,
                Action = action,
                EntityType = entityType,
                UserId = userId,
                EntityId = entityId,
                FromDate = fromDate,
                ToDate = toDate
            };

            var deletedCount = await _auditService.DeleteLogsAsync(filter);
            var filterText = AuditService.DescribeFilter(filter);

            await _auditService.LogAsync(
                "Clear",
                "AuditLog",
                null,
                "Журнал действий",
                _currentUser.UserId,
                _currentUser.Login,
                _currentUser.FullName,
                null,
                null,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                HttpContext.Request.Headers.UserAgent.ToString(),
                $"Очищен журнал: удалено {deletedCount} записей ({filterText}).");

            return Ok(new ClearAuditLogsResultDto { DeletedCount = deletedCount });
        }
    }
}
