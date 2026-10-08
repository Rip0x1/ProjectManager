using System.Windows;
using System.Windows.Input;
using ProjectManagementSystem.WPF.Models;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class AuditLogDetailsWindow : Window
    {
        public AuditLogDetailsWindow(AuditLogItem log)
        {
            InitializeComponent();
            DataContext = new AuditLogDetailsModel(log);
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private sealed class AuditLogDetailsModel
        {
            public AuditLogDetailsModel(AuditLogItem log)
            {
                Id = log.Id;
                TimestampText = log.TimestampText;
                UserDisplay = log.UserDisplay;
                UserLogin = string.IsNullOrWhiteSpace(log.UserLogin) ? "—" : log.UserLogin;
                ActionText = log.ActionText;
                EntityTypeText = log.EntityTypeText;
                EntityIdText = log.EntityIdText;
                EntityName = string.IsNullOrWhiteSpace(log.EntityName) ? "—" : log.EntityName;
                Details = string.IsNullOrWhiteSpace(log.Details) ? "Нет описания действия" : log.Details;
            }

            public int Id { get; }
            public string TimestampText { get; }
            public string UserDisplay { get; }
            public string UserLogin { get; }
            public string ActionText { get; }
            public string EntityTypeText { get; }
            public string EntityIdText { get; }
            public string EntityName { get; }
            public string Details { get; }
        }
    }
}
