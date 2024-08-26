using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoApp.Api.Controllers;
using TodoApp.Application.Entities;
using TodoApp.Application.Services;
using Xunit;

namespace TodoApp.Test.Mocking
{
    public class MoqApiTests
    {
        #region Props
        private readonly Mock<INotificationService> _moqNotificationService;
        private readonly Mock<ITodoService> _moqTodoService;
        #endregion
        #region Constructor
        public MoqApiTests()
        {
            _moqTodoService = new Mock<ITodoService>();
            _moqNotificationService = new Mock<INotificationService>();
        }
        #endregion
        #region Get
        [Fact]
        public void GetAll_ReturnsExpectedData()
        {
            // Arrange
            List<Todo> expectedTodos = new List<Todo>
            {
                new() { Id = Guid.NewGuid(), Description = "Task 1", IsCompleted = false},
                new() { Id = Guid.NewGuid(), Description = "Task 2", IsCompleted = true}
            };
            _moqTodoService.Setup(s => s.GetAllTodos()).Returns(expectedTodos);
            TodoController sut = new TodoController(_moqTodoService.Object, _moqNotificationService.Object);
            // Act
            var res = sut.GetAllTodoItems();
            var okObjectResult = (OkObjectResult)res;
            // Assert
            okObjectResult.Value.Should().BeEquivalentTo(expectedTodos);
            okObjectResult.StatusCode.Should().Be(200);
        }
        [Fact]
        public void GetAll_ReturnsEmptyArray_WhenNoItems()
        {
            // Arrange
            List<Todo> expectedTodos = [];
            _moqTodoService.Setup(s => s.GetAllTodos()).Returns(expectedTodos);
            TodoController sut = new TodoController(_moqTodoService.Object, _moqNotificationService.Object);
            // Act
            IActionResult res = sut.GetAllTodoItems();
            OkObjectResult okObjectResult = (OkObjectResult)res;
            // Assert
            okObjectResult.Value.Should().BeEquivalentTo(expectedTodos);
            okObjectResult.StatusCode.Should().Be(200);
        }
        #endregion
        #region Delete
        [Fact]
        public void Delete_Returns500_AndErrorMessageThrown_WhenExceptionThrown()
        {
            // Arrange
            const string errorMessage = "Faild to delete item id doesn't exist";
            Guid id = Guid.NewGuid();
            _moqTodoService.Setup(s => s.DeleteTodo(It.IsAny<Guid>())).Throws(new Exception(errorMessage));
            TodoController sut = new TodoController(_moqTodoService.Object, _moqNotificationService.Object);
            // Act
            IActionResult res = sut.DeleteTodoItem(id);
            ObjectResult objectResult = (ObjectResult)res;
            // Assert
            objectResult.Should().NotBeNull();
            objectResult.StatusCode.Should().Be(500);
            (objectResult.Value as ProblemDetails)?.Detail.Should().Be(errorMessage);
        }
        [Fact]
        public void DeleteAPI_CallsNotificationService_WithTaskId_AndUserId()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            _moqTodoService.Setup(x=>x.DeleteTodo(id)).Verifiable();
            // Act
            var res = new TodoController(_moqTodoService.Object, _moqNotificationService.Object).DeleteTodoItem(id);
            // Assert
            _moqNotificationService.Verify(x => x.NotifyUserTaskDeleted(id,1));// Defaults to Times.AlLeastOnce
        }
        #endregion
    }
}
