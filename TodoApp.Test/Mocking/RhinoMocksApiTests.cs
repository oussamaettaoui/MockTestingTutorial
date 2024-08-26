using Microsoft.AspNetCore.Mvc;
using Rhino.Mocks;
using TodoApp.Api.Controllers;
using TodoApp.Application.Entities;
using TodoApp.Application.Services;
using FluentAssertions;
using Xunit;

namespace TodoApp.Test.Mocking
{
    public class RhinoMocksApiTests
    {
        private readonly ITodoService _mockTodoService;
        private readonly INotificationService _mockNotificationService;

        public RhinoMocksApiTests()
        {
            _mockTodoService = MockRepository.GenerateMock<ITodoService>();
            _mockNotificationService = MockRepository.GenerateMock<INotificationService>();
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
            _mockTodoService.Stub(s => s.GetAllTodos()).Return(expectedTodos);
            var sut = new TodoController(_mockTodoService, _mockNotificationService);
            // Act
            var response = sut.GetAllTodoItems();
            // Assert
            var okObjectResult = response.Should().BeOfType<OkObjectResult>().Subject;
            okObjectResult.Value.Should().BeEquivalentTo(expectedTodos);
            okObjectResult.StatusCode.Should().Be(200);
        }
        [Fact]
        public void GetAll_ReturnsEmptyArray_WhenNoItems()
        {
            // Arrange
            var expectedTodos = new List<Todo>();
            _mockTodoService.Stub(s => s.GetAllTodos()).Return(expectedTodos);
            var sut = new TodoController(_mockTodoService, _mockNotificationService);
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
            _mockTodoService.Stub(s => s.DeleteTodo(Guid.NewGuid())).Throw(new Exception(errorMessage));
            _mockTodoService.Stub(s => s.DeleteTodo(Arg<Guid>.Is.Anything)).Throw(new Exception(errorMessage));
            TodoController sut = new TodoController(_mockTodoService, _mockNotificationService);
            // Act
            IActionResult response = sut.DeleteTodoItem(Guid.NewGuid());
            // cast
            ObjectResult okObjectResult = (ObjectResult)response;
            // Assert
            okObjectResult.StatusCode.Should().Be(500);
            (okObjectResult.Value as ProblemDetails)?.Detail.Should().Be(errorMessage);
        }
        [Fact]
        public void DeleteAPI_CallsNotificationService_WithTaskId_AndUserId()
        {
            Guid id = Guid.Parse("abfbef60-8ea1-4cc0-aa1d-7d5c2fd18bf0");
            // Act
            IActionResult result = new TodoController(_mockTodoService, _mockNotificationService).DeleteTodoItem(id);
            // Assert
            _mockNotificationService.AssertWasCalled(x => x.NotifyUserTaskDeleted(id, 1));
        }
        #endregion
    }
}
