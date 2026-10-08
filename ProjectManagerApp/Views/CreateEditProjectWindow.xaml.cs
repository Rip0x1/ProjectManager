using System.Windows;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.ViewModels;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class CreateEditProjectWindow : Window
    {
        public CreateEditProjectWindow(CreateEditProjectViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.CloseRequested += (_, success) =>
            {
                DialogResult = success;
                Close();
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
