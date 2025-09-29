using System.Windows;
using ProjectManagementSystem.WPF.Models;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class TaskDetailsWindow : Window
    {
        public TaskDetailsWindow(TaskItem task)
        {
            InitializeComponent();
            DataContext = task;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
