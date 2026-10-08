using System.Threading.Tasks;
using System.Windows;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.ViewModels;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class UserDetailsWindow : Window
    {
        public UserDetailsWindow()
        {
            InitializeComponent();
        }

        public async Task PrepareAsync(UserItem user)
        {
            var viewModel = App.GetService<UserDetailsViewModel>();
            DataContext = viewModel;
            await viewModel.InitializeAsync(user);
            WindowFitHelper.ApplySizeBeforeShow(this, 600, 560);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
