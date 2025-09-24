using MaterialDesignThemes.Wpf;

namespace ProjectManagementSystem.WPF.Services
{
    public interface INotificationService
    {
        void ShowSuccess(string message);
        void ShowError(string message);
        void ShowWarning(string message);
        void ShowInfo(string message);
        SnackbarMessageQueue MessageQueue { get; } // Добавьте это свойство
    }

    public class NotificationService : INotificationService
    {
        private readonly SnackbarMessageQueue _messageQueue;

        public NotificationService()
        {
            _messageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(2));
        }

        public SnackbarMessageQueue MessageQueue => _messageQueue;

        public void ShowSuccess(string message)
        {
            MessageQueue.Enqueue("✓ " + message); 
        }

        public void ShowError(string message)
        {
            MessageQueue.Enqueue("✗ " + message);
        }

        public void ShowWarning(string message)
        {
            MessageQueue.Enqueue("⚠ " + message);
        }

        public void ShowInfo(string message)
        {
            MessageQueue.Enqueue("ℹ " + message);
        }
    }
}