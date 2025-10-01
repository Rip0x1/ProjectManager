namespace ProjectManagementSystem.WPF.Models
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public int Priority { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public int? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal? PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public int CommentsCount { get; set; }
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public int Priority { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public int? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal? PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public int CommentsCount { get; set; }

        // Вычисляемые свойства
        public string StatusText => Status switch
        {
            0 => "Новая",
            1 => "В работе",
            2 => "На проверке",
            3 => "Завершена",
            _ => "Неизвестно"
        };

        public string PriorityText => Priority switch
        {
            0 => "Низкий",
            1 => "Средний",
            2 => "Высокий",
            3 => "Критический",
            _ => "Неизвестно"
        };

        public string StatusColor => Status switch
        {
            0 => "#2196F3", // Новая - Синий
            1 => "#FFC107", // В работе - Желтый
            2 => "#FF9800", // На проверке - Оранжевый
            3 => "#4CAF50", // Завершена - Зеленый
            _ => "#9E9E9E"  // Неизвестно - Серый
        };

        public string PriorityColor => Priority switch
        {
            0 => "#4CAF50", // Низкий - Зеленый
            1 => "#FFC107", // Средний - Желтый
            2 => "#FF9800", // Высокий - Оранжевый
            3 => "#F44336", // Критический - Красный
            _ => "#9E9E9E"  // Неизвестно - Серый
        };

        public string CreatedAtText => CreatedAt.ToString("dd.MM.yyyy HH:mm");
        public string UpdatedAtText => UpdatedAt.ToString("dd.MM.yyyy HH:mm");
        public string PlannedHoursText => PlannedHours.HasValue ? $"{PlannedHours.Value:0.##} ч." : "Не указано";
        public string ActualHoursText => ActualHours.HasValue ? $"{ActualHours.Value:0.##} ч." : "Не указано";
        public string AssigneeDisplayName => string.IsNullOrEmpty(AssigneeName) ? "Не назначено" : AssigneeName;
    }

    // DTO для создания и обновления задачи
    public class CreateUpdateTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public int Priority { get; set; }
        public int ProjectId { get; set; }
        public int AuthorId { get; set; }
        public int? AssigneeId { get; set; }
        public decimal? PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }
    }
}
