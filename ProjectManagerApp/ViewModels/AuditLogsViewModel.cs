using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using ProjectManagementSystem.WPF.Views;

namespace ProjectManagementSystem.WPF.ViewModels
{
    public partial class AuditLogsViewModel : ObservableObject
    {
        private readonly IAuditLogsService _auditLogsService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IDataRefreshService _dataRefreshService;
        private bool _suppressReload;
        private int _loadVersion;

        [ObservableProperty]
        private ObservableCollection<AuditLogItem> _logs = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private int _pageSize = 25;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private string _selectedAction = "All";

        [ObservableProperty]
        private string _selectedEntityType = "All";

        [ObservableProperty]
        private DateTime? _filterFrom;

        [ObservableProperty]
        private DateTime? _filterTo;

        public ObservableCollection<ColumnVisibilityOption> ColumnOptions { get; } = new()
        {
            new ColumnVisibilityOption("Id", "ID"),
            new ColumnVisibilityOption("TimestampText", "Время"),
            new ColumnVisibilityOption("UserDisplay", "Пользователь"),
            new ColumnVisibilityOption("ActionText", "Действие"),
            new ColumnVisibilityOption("EntityTypeText", "Объект"),
            new ColumnVisibilityOption("EntityIdText", "ID объекта"),
            new ColumnVisibilityOption("EntityName", "Название"),
            new ColumnVisibilityOption("Details", "Детали")
        };

        public AuditLogsViewModel(
            IAuditLogsService auditLogsService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IDataRefreshService dataRefreshService)
        {
            _auditLogsService = auditLogsService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _dataRefreshService = dataRefreshService;
            _dataRefreshService.Changed += OnDataRefresh;
        }

        private void OnDataRefresh(object? sender, DataRefreshScope scope)
        {
            if (scope == DataRefreshScope.None)
            {
                return;
            }

            Application.Current?.Dispatcher.BeginInvoke(() => RefreshAfterChange());
        }

        public void RefreshAfterChange()
        {
            if (CurrentPage != 1)
            {
                CurrentPage = 1;
                return;
            }

            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            if (_suppressReload || !_permissionService.IsAdmin())
            {
                return;
            }

            var version = ++_loadVersion;
            var showOverlay = Logs.Count == 0;
            try
            {
                if (showOverlay)
                {
                    IsLoading = true;
                }

                var response = await _auditLogsService.GetLogsAsync(
                    SearchText,
                    SelectedAction is null or "" or "All" ? null : SelectedAction,
                    SelectedEntityType is null or "" or "All" ? null : SelectedEntityType,
                    FilterFrom,
                    FilterTo,
                    CurrentPage,
                    PageSize);

                if (version != _loadVersion)
                {
                    return;
                }

                _suppressReload = true;
                TotalCount = response.TotalCount;
                TotalPages = response.TotalPages < 1 ? 1 : response.TotalPages;
                if (CurrentPage > TotalPages)
                {
                    CurrentPage = TotalPages;
                }

                Logs = new ObservableCollection<AuditLogItem>(response.Items.Select(Map));
            }
            catch (Exception ex)
            {
                if (version == _loadVersion)
                {
                    _notificationService.ShowError($"Ошибка загрузки журнала: {ex.Message}");
                }
            }
            finally
            {
                if (version == _loadVersion)
                {
                    _suppressReload = false;
                    IsLoading = false;
                }
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            var sanitized = InputValidation.SanitizeSearch(value);
            if (sanitized != value)
            {
                SearchText = sanitized;
                return;
            }

            CurrentPage = 1;
            _ = LoadAsync();
        }

        partial void OnPageSizeChanged(int value)
        {
            CurrentPage = 1;
            _ = LoadAsync();
        }

        partial void OnCurrentPageChanged(int value)
        {
            var clamped = InputValidation.ClampPage(value, TotalPages);
            if (clamped != value)
            {
                CurrentPage = clamped;
                return;
            }

            _ = LoadAsync();
        }

        partial void OnSelectedActionChanged(string value)
        {
            CurrentPage = 1;
            _ = LoadAsync();
        }

        partial void OnSelectedEntityTypeChanged(string value)
        {
            CurrentPage = 1;
            _ = LoadAsync();
        }

        partial void OnFilterFromChanged(DateTime? value)
        {
            CurrentPage = 1;
            _ = LoadAsync();
        }

        partial void OnFilterToChanged(DateTime? value)
        {
            CurrentPage = 1;
            _ = LoadAsync();
        }

        [RelayCommand]
        private async Task ClearLogsAsync()
        {
            if (!_permissionService.IsAdmin())
            {
                return;
            }

            if (TotalCount <= 0)
            {
                _notificationService.ShowInfo("Нет записей для удаления");
                return;
            }

            var filterText = BuildClearFilterSummary();
            var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                ?? Application.Current?.MainWindow;
            if (!ConfirmClearLogsWindow.Confirm(owner, filterText, TotalCount))
            {
                return;
            }

            try
            {
                IsLoading = true;
                var deleted = await _auditLogsService.ClearLogsAsync(
                    SearchText,
                    SelectedAction is null or "" or "All" ? null : SelectedAction,
                    SelectedEntityType is null or "" or "All" ? null : SelectedEntityType,
                    FilterFrom,
                    FilterTo);

                _notificationService.ShowSuccess($"Удалено записей журнала: {deleted}");
                _suppressReload = true;
                CurrentPage = 1;
                _suppressReload = false;
                await LoadAsync();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка очистки журнала: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string BuildClearFilterSummary()
        {
            var parts = new List<string>();

            if (FilterFrom.HasValue)
            {
                parts.Add($"с {FilterFrom.Value:dd.MM.yyyy}");
            }

            if (FilterTo.HasValue)
            {
                parts.Add($"по {FilterTo.Value:dd.MM.yyyy}");
            }

            if (SelectedAction is not (null or "" or "All"))
            {
                parts.Add("действие: " + MapAction(SelectedAction));
            }

            if (SelectedEntityType is not (null or "" or "All"))
            {
                parts.Add("объект: " + MapEntityType(SelectedEntityType));
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                parts.Add($"поиск «{SearchText.Trim()}»");
            }

            return parts.Count == 0
                ? "Фильтры не заданы — будут удалены все записи журнала."
                : "Удаляются записи по условиям: " + string.Join(", ", parts) + ".";
        }

        private static string MapAction(string action) => action switch
        {
            "Create" => "Создание",
            "Update" => "Изменение",
            "Delete" => "Удаление",
            "Login" => "Вход",
            "Logout" => "Выход",
            "LoginFailed" => "Неудачный вход",
            "Clear" => "Очистка журнала",
            _ => action
        };

        private static string MapEntityType(string entityType) => entityType switch
        {
            "Project" => "Проект",
            "Task" => "Задача",
            "User" => "Пользователь",
            "Comment" => "Комментарий",
            "ProjectUser" => "Участник проекта",
            "TaskAssignee" => "Исполнитель",
            "TaskAttachment" => "Вложение задачи",
            "ProjectAttachment" => "Вложение проекта",
            "CommentAttachment" => "Вложение комментария",
            "AuditLog" => "Журнал",
            _ => entityType
        };

        [RelayCommand]
        private void ClearFilters()
        {
            _suppressReload = true;
            SelectedAction = "All";
            SelectedEntityType = "All";
            FilterFrom = null;
            FilterTo = null;
            SearchText = string.Empty;
            CurrentPage = 1;
            _suppressReload = false;
            _ = LoadAsync();
            _notificationService.ShowInfo("Фильтры сброшены");
        }

        [RelayCommand]
        private void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
        }

        [RelayCommand]
        private void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
            }
        }

        [RelayCommand]
        private void FirstPage()
        {
            CurrentPage = 1;
        }

        [RelayCommand]
        private void LastPage()
        {
            CurrentPage = TotalPages;
        }

        private static AuditLogItem Map(AuditLogDto dto)
        {
            return new AuditLogItem
            {
                Id = dto.Id,
                UserId = dto.UserId,
                UserLogin = dto.UserLogin,
                UserName = dto.UserName,
                Action = dto.Action,
                ActionText = dto.ActionText,
                EntityType = dto.EntityType,
                EntityTypeText = dto.EntityTypeText,
                EntityId = dto.EntityId,
                EntityName = dto.EntityName,
                IpAddress = dto.IpAddress,
                UserAgent = dto.UserAgent,
                Timestamp = dto.Timestamp,
                Details = dto.Details
            };
        }
    }
}
