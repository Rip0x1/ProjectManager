using System.Windows;
using System.Windows.Input;
using ProjectManagerApp.ViewModels;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Views;

namespace ProjectManagerApp.Views
{
    public partial class ProjectTasksWindow : Window
    {
        public ProjectTasksWindow()
        {
            InitializeComponent();
        }

        private async void TaskCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2 || sender is not FrameworkElement element || element.DataContext is not TaskItem task)
            {
                return;
            }

            e.Handled = true;
            await OpenTaskDetailsAsync(task);
        }

        private async System.Threading.Tasks.Task OpenTaskDetailsAsync(TaskItem task)
        {
            if (DataContext is ProjectTasksViewModel viewModel)
            {
                if (viewModel.ViewTaskDetailsCommand.CanExecute(task))
                {
                    viewModel.ViewTaskDetailsCommand.Execute(task);
                }
            }
            else
            {
                var detailsWindow = new TaskDetailsWindow { Owner = this };
                await detailsWindow.PrepareAsync(task);
                detailsWindow.ShowDialog();
            }
        }

        private void DragWindow_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}


