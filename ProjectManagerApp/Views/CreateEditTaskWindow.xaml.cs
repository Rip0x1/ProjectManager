using System.Windows;
using ProjectManagementSystem.WPF.ViewModels;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class CreateEditTaskWindow : Window
    {
        public CreateEditTaskWindow(int? taskId = null, int? projectId = null, string projectName = null)
        {
            InitializeComponent();
            
            var viewModel = App.GetService<CreateEditTaskViewModel>();
            DataContext = viewModel;
            
            viewModel.CloseRequested += (s, success) =>
            {
                DialogResult = success;
                Close();
            };
            
            Loaded += async (s, e) => 
            {
                if (taskId.HasValue)
                {
                    await viewModel.InitializeForEdit(taskId.Value);
                }
                else if (projectId.HasValue && !string.IsNullOrEmpty(projectName))
                {
                    await viewModel.InitializeForProject(projectId.Value, projectName);
                }
                else
                {
                    await viewModel.LoadAsync();
                }
            };
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

