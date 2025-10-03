using System.Windows.Controls;
using System.Windows;
using ProjectManagerApp.ViewModels;

namespace ProjectManagerApp.Views
{
    public partial class MyProjectsView : UserControl
    {
        public MyProjectsView()
        {
            InitializeComponent();
            Loaded += MyProjectsView_Loaded;
        }

        private async void MyProjectsView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MyProjectsViewModel viewModel)
            {
                await viewModel.LoadAsync();
            }
        }
    }
}
