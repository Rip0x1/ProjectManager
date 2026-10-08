using System.Windows;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.ViewModels;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class TaskDetailsWindow : Window
    {
        public TaskDetailsWindow()
        {
            InitializeComponent();
        }

        public async Task PrepareAsync(TaskItem task)
        {
            var viewModel = App.GetService<TaskDetailsViewModel>();
            DataContext = viewModel;
            await viewModel.InitializeAsync(task);
            WindowFitHelper.ApplySizeBeforeShow(this, 640, 680);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
