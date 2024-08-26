using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Api.Controllers;
using TodoApp.Application.Entities;
using TodoApp.Application.Services;
using Xunit;

namespace TodoApp.Test.Mocking
{
    public class FakeItEasyApiTests
    {
        #region Props
        private readonly ITodoService _fakeTodoService;
        private readonly INotificationService _fakeNotificationService;
        #endregion
        #region Constructor
        public FakeItEasyApiTests()
        {
            _fakeTodoService = A.Fake<ITodoService>();
            _fakeNotificationService = A.Fake<INotificationService>();
        }
        #endregion
        #region Get
        [Fact]
        public void GetAll_ReturnsExpectedData()
        {
            List<Todo> expectedTodos = new List<Todo>
            {
                new() { Id = Guid.NewGuid(), Description = "Task 1", IsCompleted = false},
                new() { Id = Guid.NewGuid(), Description = "Task 2", IsCompleted = true}
            };
            // Arrange
            A.CallTo(() => _fakeTodoService.GetAllTodos()).Returns(expectedTodos);
            TodoController sut = new TodoController(_fakeTodoService,_fakeNotificationService);
            // Act
            IActionResult res = sut.GetAllTodoItems();
            // Assert
            OkObjectResult okObjectResult = res.Should().BeOfType<OkObjectResult>().Subject;
            okObjectResult.Value.Should().BeEquivalentTo(expectedTodos);
            okObjectResult.StatusCode.Should().Be(200);
        }
        [Fact]
        public void GetAll_ReturnsEmptyArray_WhenNoItems()
        {
            // Arrage
            var expectedTodos = new List<Todo>();
            A.CallTo(() => _fakeTodoService.GetAllTodos()).Returns(expectedTodos);
            TodoController sut = new TodoController(_fakeTodoService, _fakeNotificationService);
            // Act
            IActionResult res = sut.GetAllTodoItems();
            // Assert
            OkObjectResult okObjectResult = res.Should().BeOfType<OkObjectResult>().Subject;
            okObjectResult.Value.Should().BeEquivalentTo(expectedTodos);
            okObjectResult.StatusCode.Should().Be(200);
        }
        #endregion
        #region Delete
        [Fact]
        public void Delete_Returns500_AndErrorMessageThrown_WhenExceptionThrown()
        {
            // Arrange
            const string errorMessage = "Fail to delete It id deosn't exist";
            A.CallTo(() => _fakeTodoService.DeleteTodo(Guid.NewGuid())).Throws(new Exception(errorMessage));
            A.CallTo(() => _fakeTodoService.DeleteTodo(A<Guid>._)).Throws(new Exception(errorMessage));
            TodoController sut = new TodoController(_fakeTodoService,_fakeNotificationService);
            // Act
            var res = sut.DeleteTodoItem(Guid.NewGuid());
            var okObjectResult = (ObjectResult)res;
            //Assert
            okObjectResult.StatusCode.Should().Be(500);
            (okObjectResult.Value as ProblemDetails)?.Detail.Should().Be(errorMessage);
        }
        [Fact]
        public void DeleteAPI_CallsNotificationService_WithTaskId_AndUserId()
        {
            // Arrange
            A.CallTo(() => _fakeTodoService.DeleteTodo(A<Guid>._)).Returns(Result.Success);
            // Act
            Guid id = Guid.NewGuid();
            var res = new TodoController(_fakeTodoService,_fakeNotificationService).DeleteTodoItem(id);
            // Assert
            A.CallTo(() => _fakeNotificationService.NotifyUserTaskDeleted(id, 1)).MustHaveHappened(1, Times.Exactly);
        }
        #endregion
    }
}
