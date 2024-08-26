using TodoApp.Application.Entities;

namespace TodoApp.Application.Services
{
    public class TodoService : ITodoService
    {
        private readonly List<Todo> _todos = new List<Todo>()
        {
            new Todo
            {
                Id = Guid.NewGuid(),
                Description = "todo1",
                IsCompleted = false
            }
        };
        public List<Todo> GetAllTodos()
        {
            return _todos;
        }
        public Todo AddTodo(string description)
        {
            Todo todo = new Todo { Id = Guid.NewGuid(), Description = description, IsCompleted = false };
            _todos.Add(todo);
            return todo;
        }
        public Result DeleteTodo(Guid id)
        {
            try
            {
               if(_todos.Any(x => x.Id == id))
                {
                    _todos.RemoveAll(x => x.Id == id);
                    return Result.Success;
                }
                else
                {
                    return Result.NotFound;
                }
            }catch(Exception ex)
            {
               return Result.Failure;
            }
        }
    }
}
