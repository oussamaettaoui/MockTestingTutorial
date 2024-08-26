namespace TodoApp.Application.Entities
{
    public class Todo
    {
        public Guid Id { get; set; }
        public string? Description { get; init; }
        public bool IsCompleted { get; set; }
    }
}
