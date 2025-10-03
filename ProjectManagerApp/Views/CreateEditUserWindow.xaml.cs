using ProjectManagerApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ProjectManagerApp.Views
{
    public partial class CreateEditUserWindow : Window
    {
        public CreateEditUserWindow(CreateEditUserViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            if (viewModel.IsEditMode)
            {
                PasswordBox.Password = viewModel.Password;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is CreateEditUserViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is CreateEditUserViewModel viewModel)
            {
                viewModel.ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}