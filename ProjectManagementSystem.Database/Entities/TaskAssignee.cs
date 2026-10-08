namespace ProjectManagementSystem.Database.Entities
{
    public class TaskAssignee
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }

        public virtual Task Task { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
