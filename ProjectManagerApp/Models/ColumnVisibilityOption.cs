using CommunityToolkit.Mvvm.ComponentModel;

namespace ProjectManagementSystem.WPF.Models
{
    public partial class ColumnVisibilityOption : ObservableObject
    {
        public ColumnVisibilityOption(string key, string header, bool isVisible = true, bool isLocked = false)
        {
            Key = key;
            Header = header;
            _isVisible = isVisible;
            IsLocked = isLocked;
        }

        public string Key { get; }
        public string Header { get; }
        public bool IsLocked { get; }

        [ObservableProperty]
        private bool _isVisible;
    }
}
