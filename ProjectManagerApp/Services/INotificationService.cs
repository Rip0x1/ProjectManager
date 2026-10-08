using MaterialDesignThemes.Wpf;

namespace ProjectManagementSystem.WPF.Services
{
    public interface INotificationService
    {
        void ShowSuccess(string message);
        void ShowError(string message);
        void ShowWarning(string message);
        void ShowInfo(string message);
        SnackbarMessageQueue MessageQueue { get; set; }
        void SetNotificationBorder(System.Windows.Controls.Border border);
    }

    public class NotificationService : INotificationService
    {
        private SnackbarMessageQueue _messageQueue;
        private System.Windows.Controls.Border _notificationBorder;

        public NotificationService()
        {
            _messageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(2));
        }

        public SnackbarMessageQueue MessageQueue 
        { 
            get => _messageQueue; 
            set => _messageQueue = value; 
        }

        public void SetNotificationBorder(System.Windows.Controls.Border border)
        {
            _notificationBorder = border;
        }

        public void ShowSuccess(string message)
        {
            SetNotificationColor("#4CAF50");
            MessageQueue?.Enqueue(message); 
        }

        public void ShowError(string message)
        {
            SetNotificationColor("#F44336");
            MessageQueue?.Enqueue(message);
        }

        public void ShowWarning(string message)
        {
            SetNotificationColor("#FF9800");
            MessageQueue?.Enqueue(message);
        }

        public void ShowInfo(string message)
        {
            SetNotificationColor("#2196F3");
            MessageQueue?.Enqueue(message);
        }

        private void SetNotificationColor(string color)
        {
            if (_notificationBorder != null)
            {
                _notificationBorder.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
            }
        }
    }
}