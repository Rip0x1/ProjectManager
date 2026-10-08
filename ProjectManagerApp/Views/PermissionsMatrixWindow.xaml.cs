using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class PermissionsMatrixWindow : Window
    {
        public PermissionsMatrixWindow()
        {
            InitializeComponent();

            var auth = App.GetService<IAuthService>();
            var role = (UserRole)auth.CurrentUserRole;
            var roleName = role.GetRoleName();

            RoleInfoText.Text =
                $"Ваша роль: {roleName}. Столбец вашей роли выделен. " +
                "«Да» — полный доступ; «Только свои» / «Только свой проект» — ограниченный; «Нет» — недоступно.";

            PermissionsGrid.ItemsSource = PermissionMatrixCatalog.GetRows();
            HighlightRoleColumn(role);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FitWindowToScreen();
        }

        private void FitWindowToScreen()
        {
            var workArea = SystemParameters.WorkArea;
            Width = Math.Max(MinWidth, Math.Min(1180, workArea.Width * 0.9));
            Height = Math.Max(MinHeight, Math.Min(820, workArea.Height * 0.9));

            if (Owner != null)
            {
                Left = Owner.Left + (Owner.Width - Width) / 2;
                Top = Owner.Top + (Owner.Height - Height) / 2;
            }
            else
            {
                Left = workArea.Left + (workArea.Width - Width) / 2;
                Top = workArea.Top + (workArea.Height - Height) / 2;
            }
        }

        private void HighlightRoleColumn(UserRole role)
        {
            var highlightBrush = new SolidColorBrush(Color.FromRgb(0xE8, 0xEA, 0xF6));
            var headerBrush = new SolidColorBrush(Color.FromRgb(0x5E, 0x35, 0xB1));
            var whiteBrush = Brushes.White;

            DataGridColumn? target = role switch
            {
                UserRole.User => ColUser,
                UserRole.Manager => ColManager,
                UserRole.Admin => ColAdmin,
                _ => null
            };

            if (target == null)
            {
                return;
            }

            target.HeaderStyle = CreateHeaderStyle(headerBrush, whiteBrush, true);
            target.CellStyle = CreateCellStyle(highlightBrush);
        }

        private Style CreateHeaderStyle(Brush background, Brush foreground, bool bold)
        {
            var style = new Style(typeof(DataGridColumnHeader));
            style.BasedOn = (FindResource("MatrixDataGridColumnHeaderStyle") as Style)
                ?? Application.Current.FindResource("AppDataGridColumnHeaderStyle") as Style;
            style.Setters.Add(new Setter(Control.BackgroundProperty, background));
            style.Setters.Add(new Setter(Control.ForegroundProperty, foreground));
            if (bold)
            {
                style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
            }

            return style;
        }

        private Style CreateCellStyle(Brush background)
        {
            var style = new Style(typeof(DataGridCell));
            style.BasedOn = (FindResource("MatrixDataGridCellStyle") as Style)
                ?? Application.Current.FindResource("AppDataGridCellStyle") as Style;
            style.Setters.Add(new Setter(Control.BackgroundProperty, background));
            return style;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
