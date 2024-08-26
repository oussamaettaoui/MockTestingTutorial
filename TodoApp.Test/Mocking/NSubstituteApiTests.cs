using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TodoApp.Api.Controllers;
using TodoApp.Application.Entities;
using TodoApp.Application.Services;
using FluentAssertions;
using Xunit;

namespace TodoApp.Test.Mocking
{
    public class NSubstituteApiTests
    {
        private readonly ITodoService _substituteTodoService;
        private readonly INotificationService _notificationService;

        public NSubstituteApiTests()
        {
            _substituteTodoService = Substitute.For<ITodoService>();
            _notificationService = Substitute.For<INotificationService>();
        }
        #region Get
        [Fact]
        public void GetAll_ReturnsExpectedData()
        {
            // Arrange
            var expectedTodos = new List<Todo>
            {
                new Todo { Id = Guid.NewGuid(), Description = "Task 1", IsCompleted = false },
                new Todo { Id = Guid.NewGuid(), Description = "Task 2", IsCompleted = true }
            };
            _substituteTodoService.GetAllTodos().Returns(expectedTodos);
            TodoController sut = new TodoController(_substituteTodoService, _notificationService);
            // Act
            IActionResult response = sut.GetAllTodoItems();
            // Assert
            OkObjectResult okObjectResult = response.Should().BeOfType<OkObjectResult>().Subject;
            okObjectResult.Value.Should().BeEquivalentTo(expectedTodos);
            okObjectResult.StatusCode.Should().Be(200);
        }
        [Fact]
        public void GetAll_ReturnsEmptyArray_WhenNoItems()
        {
            // Arrange
            var expectedTodos = new List<Todo>();
            _substituteTodoService.GetAllTodos().Returns(expectedTodos);
            var sut = new TodoController(_substituteTodoService, _notificationService);
            // Act
            var response = sut.GetAllTodoItems();
            // Assert
            var okObjectResult = response.Should().BeOfType<OkObjectResult>().Subject;
            okObjectResult.Value.Should().BeEquivalentTo(expectedTodos);
            okObjectResult.StatusCode.Should().Be(200);
        }
        #endregion
        #region Delete
        [Fact]
        public void Delete_Returns500_AndErrorMessageThrown_WhenExceptionThrown()
        {
            // Arrange
            const string errorMessage = "Failed to delete item id doesn't exist";
            _substituteTodoService.When(x => x.DeleteTodo(Guid.NewGuid())).Do(x => throw new Exception(errorMessage));
            _substituteTodoService.When(x => x.DeleteTodo(Arg.Any<Guid>())).Do(x => throw new Exception(errorMessage));
            var sut = new TodoController(_substituteTodoService, _notificationService);
            // Act
            var response = sut.DeleteTodoItem(Guid.NewGuid());
            // cast
            var okObjectResult = (ObjectResult)response;
            // Assert
            okObjectResult.StatusCode.Should().Be(500);
            (okObjectResult.Value as ProblemDetails)?.Detail.Should().Be(errorMessage);
        }

        [Fact]
        public void DeleteAPI_CallsNotificationService_WithTaskId_AndUserId()
        {
            Guid id = Guid.Parse("abfbef60-8ea1-4cc0-aa1d-7d5c2fd18bf0");
            // Act
            var result = new TodoController(_substituteTodoService, _notificationService).DeleteTodoItem(id);
            // Assert
            _notificationService.Received().NotifyUserTaskDeleted(id, 1);
        }
        #endregion
    }
}
