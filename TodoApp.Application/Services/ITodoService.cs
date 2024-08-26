using TodoApp.Application.Entities;

namespace TodoApp.Application.Services
{
    public interface ITodoService
    {
        List<Todo> GetAllTodos();
        Todo AddTodo(string description);
        Result DeleteTodo(Guid id);
    }
}
