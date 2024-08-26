using Microsoft.AspNetCore.Mvc;
using TodoApp.Application.Entities;
using TodoApp.Application.Services;

namespace TodoApp.Api.Controllers
{
    [Route("api/Todo")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;
        private readonly INotificationService _notificationService;

        public TodoController(ITodoService todoService, INotificationService notificationService)
        {
            _todoService = todoService;
            _notificationService = notificationService;
        }
        [HttpGet]
        public IActionResult GetAllTodoItems()
        {
            List<Todo> todos = _todoService.GetAllTodos();
            return Ok(todos);
        }
        [HttpPost]
        public IActionResult AddTodoItems([FromForm] string todoDescription)
        {
            if (string.IsNullOrWhiteSpace(todoDescription))
            {
                return BadRequest("Todo description cannot be empty.");
            }

            Todo todo = _todoService.AddTodo(todoDescription);
            return Ok(todo);
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteTodoItem(Guid id)
        {
            try
            {

                Result result = _todoService.DeleteTodo(id);
                if (result == Result.Success)
                {
                    _notificationService.NotifyUserTaskDeleted(id, 1);
                    return Ok("Todo Deleted Successfully");
                }
                return result switch
                {
                    Result.NotFound => NotFound($"Todo With Id {id} Not Found"),
                    Result.Failure => BadRequest("Failed To Delete Todo"),
                    _ => BadRequest("Invalid Response")
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails { Detail = ex.Message });
            }
        }
    }
}
