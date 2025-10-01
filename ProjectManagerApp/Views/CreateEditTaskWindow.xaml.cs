using System.Windows;
using ProjectManagementSystem.WPF.ViewModels;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class CreateEditTaskWindow : Window
    {
        public CreateEditTaskWindow(int? taskId = null)
        {
            InitializeComponent();
            
            var viewModel = App.GetService<CreateEditTaskViewModel>();
            viewModel.TaskId = taskId;
            DataContext = viewModel;
            
            viewModel.CloseRequested += (s, success) =>
            {
                DialogResult = success;
                Close();
            };
            
            Loaded += async (s, e) => await viewModel.LoadAsync();
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

