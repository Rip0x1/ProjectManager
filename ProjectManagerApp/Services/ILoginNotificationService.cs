using MaterialDesignThemes.Wpf;

namespace ProjectManagementSystem.WPF.Services
{
    public interface ILoginNotificationService
    {
        void ShowSuccess(string message);
        void ShowError(string message);
        void ShowWarning(string message);
        void ShowInfo(string message);
        SnackbarMessageQueue MessageQueue { get; set; }
    }

    public class LoginNotificationService : ILoginNotificationService
    {
        private SnackbarMessageQueue _messageQueue;

        public LoginNotificationService()
        {
            _messageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
        }

        public SnackbarMessageQueue MessageQueue 
        { 
            get => _messageQueue; 
            set => _messageQueue = value; 
        }

        public void ShowSuccess(string message)
        {
            MessageQueue.Enqueue(message); 
        }

        public void ShowError(string message)
        {
            MessageQueue.Enqueue(message);
        }

        public void ShowWarning(string message)
        {
            MessageQueue.Enqueue(message);
        }

        public void ShowInfo(string message)
        {
            MessageQueue.Enqueue(message);
        }
    }
}
