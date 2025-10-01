using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using ProjectManagementSystem.WPF.ViewModels;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class TasksView : UserControl
    {
        public TasksView()
        {
            InitializeComponent();
            
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var viewModel = App.GetService<TasksViewModel>();
                DataContext = viewModel;
                _ = viewModel.LoadAsync();
            }
        }

        private void TaskCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is TaskItem task)
            {
                var detailsWindow = new TaskDetailsWindow(task);
                detailsWindow.Owner = Window.GetWindow(this);
                detailsWindow.ShowDialog();
            }
        }
    }
}
