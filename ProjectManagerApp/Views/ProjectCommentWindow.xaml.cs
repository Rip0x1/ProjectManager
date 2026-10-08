using System.Windows;

namespace ProjectManagerApp.Views
{
    public partial class ProjectCommentWindow : Window
    {
        public ProjectCommentWindow()
        {
            InitializeComponent();
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
