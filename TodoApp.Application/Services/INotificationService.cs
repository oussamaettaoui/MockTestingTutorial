namespace TodoApp.Application.Services
{
    public interface INotificationService
    {
        void NotifyUserTaskDeleted(Guid id, int UserId);
    }
}
