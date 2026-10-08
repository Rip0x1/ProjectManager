using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using ProjectManagementSystem.WPF.Views;
using ProjectManagerApp.Services;

namespace ProjectManagementSystem.WPF.ViewModels
{
    public enum UserRelatedKind
    {
        Projects,
        AuthoredTasks,
        AssignedTasks,
        Comments
    }

    public partial class UserRelatedListViewModel : ObservableObject
    {
        private readonly IUsersService _usersService;
        private readonly IProjectMembersService _projectMembersService;
        private readonly INotificationService _notificationService;
        private readonly IDataRefreshService _dataRefreshService;
        private readonly IPermissionService _permissionService;
        private readonly List<UserRelatedItem> _allItems = new();
        private int _userId;
        private UserRelatedKind _kind;
        private const int PageSize = 10;

        [ObservableProperty]
        private string _windowTitle = string.Empty;

        [ObservableProperty]
        private string _subtitle = string.Empty;

        [ObservableProperty]
        private string _emptyText = "Нет записей";

        [ObservableProperty]
        private string _searchHint = "Поиск...";

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private ObservableCollection<UserRelatedItem> _items = new();

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _totalCount;

        public bool HasNoItems => !IsLoading && TotalCount == 0;

        partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(HasNoItems));

        public UserRelatedListViewModel(
            IUsersService usersService,
            IProjectMembersService projectMembersService,
            INotificationService notificationService,
            IDataRefreshService dataRefreshService,
            IPermissionService permissionService)
        {
            _usersService = usersService;
            _projectMembersService = projectMembersService;
            _notificationService = notificationService;
            _dataRefreshService = dataRefreshService;
            _permissionService = permissionService;
        }

        public async Task InitializeAsync(UserRelatedKind kind, int userId, string userName)
        {
            _kind = kind;
            _userId = userId;
            WindowTitle = kind switch
            {
                UserRelatedKind.Projects => "Проекты пользователя",
                UserRelatedKind.AuthoredTasks => "Созданные задачи",
                UserRelatedKind.AssignedTasks => "Назначенные задачи",
                UserRelatedKind.Comments => "Комментарии пользователя",
                _ => "Связанные записи"
            };
            Subtitle = userName;
            SearchHint = kind switch
            {
                UserRelatedKind.Projects => "Поиск по ID или названию проекта...",
                UserRelatedKind.Comments => "Поиск по ID, тексту или задаче...",
                _ => "Поиск по ID, названию или проекту..."
            };
            EmptyText = kind switch
            {
                UserRelatedKind.Projects => "Нет проектов",
                UserRelatedKind.AuthoredTasks => "Пользователь не создавал задачи",
                UserRelatedKind.AssignedTasks => "Пользователю не назначены задачи",
                UserRelatedKind.Comments => "Пользователь не оставлял комментарии",
                _ => "Нет записей"
            };
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                _allItems.Clear();

                IEnumerable<UserRelatedItem> mapped = _kind switch
                {
                    UserRelatedKind.Projects => (await _usersService.GetUserRelatedProjectsAsync(_userId)).Select(MapProject),
                    UserRelatedKind.AuthoredTasks => (await _usersService.GetUserAuthoredTasksAsync(_userId)).Select(MapTask),
                    UserRelatedKind.AssignedTasks => (await _usersService.GetUserAssignedTasksAsync(_userId)).Select(MapTask),
                    UserRelatedKind.Comments => (await _usersService.GetUserCommentsAsync(_userId)).Select(MapComment),
                    _ => Array.Empty<UserRelatedItem>()
                };

                _allItems.AddRange(mapped);
                CurrentPage = 1;
                ApplyFilterAndPaging();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка загрузки: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(HasNoItems));
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            CurrentPage = 1;
            ApplyFilterAndPaging();
        }

        partial void OnCurrentPageChanged(int value)
        {
            ApplyFilterAndPaging();
        }

        [RelayCommand]
        private void FirstPage() => CurrentPage = 1;

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
        private void LastPage() => CurrentPage = TotalPages;

        [RelayCommand]
        private async Task AddToProjectAsync(UserRelatedItem? item)
        {
            if (item == null || !item.CanAdd || _kind != UserRelatedKind.Projects)
            {
                return;
            }

            try
            {
                await _projectMembersService.AddUserToProjectAsync(item.Id, _userId);
                _dataRefreshService.Notify(DataRefreshScopes.ProjectRelated);
                _notificationService.ShowSuccess($"Пользователь добавлен в проект «{item.Title}»");
                await LoadAsync();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Не удалось добавить в проект: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task OpenItemAsync(UserRelatedItem? item)
        {
            if (item == null)
            {
                return;
            }

            try
            {
                var owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                    ?? Application.Current.MainWindow;

                if (item.ItemType == "Project")
                {
                    var window = new ProjectDetailsWindow { Owner = owner };
                    await window.PrepareAsync(new ProjectItem { Id = item.Id, Name = item.Title });
                    window.ShowDialog();
                    return;
                }

                if (item.ItemType == "Task")
                {
                    var window = new TaskDetailsWindow { Owner = owner };
                    await window.PrepareAsync(new TaskItem { Id = item.Id, Title = item.Title });
                    window.ShowDialog();
                    return;
                }

                if (item.RelatedId is > 0)
                {
                    var window = new TaskDetailsWindow { Owner = owner };
                    await window.PrepareAsync(new TaskItem { Id = item.RelatedId.Value, Title = item.Subtitle });
                    window.ShowDialog();
                    return;
                }

                _notificationService.ShowInfo("Комментарий не привязан к задаче");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Не удалось открыть запись: {ex.Message}");
            }
        }

        private void ApplyFilterAndPaging()
        {
            IEnumerable<UserRelatedItem> filtered = _allItems;
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = _allItems.Where(item =>
                    SearchHelper.MatchesId(item.Id, SearchText) ||
                    SearchHelper.MatchesId(item.RelatedId, SearchText) ||
                    SearchHelper.MatchesText(item.Title, SearchText) ||
                    SearchHelper.MatchesText(item.Subtitle, SearchText) ||
                    SearchHelper.MatchesText(item.Meta, SearchText));
            }

            var list = filtered.ToList();
            TotalCount = list.Count;
            TotalPages = Math.Max(1, (int)Math.Ceiling(list.Count / (double)PageSize));
            if (CurrentPage > TotalPages)
            {
                CurrentPage = TotalPages;
            }

            Items = new ObservableCollection<UserRelatedItem>(
                list.Skip((CurrentPage - 1) * PageSize).Take(PageSize));
            OnPropertyChanged(nameof(HasNoItems));
        }

        private UserRelatedItem MapProject(UserRelatedProjectDto dto)
        {
            var isManager = dto.IsManager;
            var isMember = dto.IsMember && !dto.IsManager;
            return new UserRelatedItem
            {
                Id = dto.Id,
                Title = dto.Name,
                Subtitle = string.IsNullOrWhiteSpace(dto.ManagerName) ? "Без руководителя" : $"Руководитель: {dto.ManagerName}",
                Badge = isManager ? "Руководитель" : (isMember ? "Участник" : string.Empty),
                BadgeColor = isManager ? "#5E35B1" : "#1565C0",
                CardBackground = isManager ? "#EDE7F6" : (dto.IsMember ? "#E3F2FD" : "#FFFFFF"),
                CanAdd = !dto.IsManager && !dto.IsMember && _permissionService.CanEditProject(dto.ManagerId),
                Meta = $"Создан {dto.CreatedAt.ToLocalTime():dd.MM.yyyy} · {MapProjectStatus(dto.Status)}",
                ItemType = "Project"
            };
        }

        private static UserRelatedItem MapTask(UserRelatedTaskDto dto)
        {
            var task = new TaskItem { Status = dto.Status, Priority = dto.Priority };
            return new UserRelatedItem
            {
                Id = dto.Id,
                Title = dto.Title,
                Subtitle = string.IsNullOrWhiteSpace(dto.ProjectName) ? "Без проекта" : dto.ProjectName,
                Badge = task.StatusText,
                BadgeColor = task.StatusColor,
                Meta = $"Приоритет: {task.PriorityText} · {dto.CreatedAt.ToLocalTime():dd.MM.yyyy}",
                ItemType = "Task",
                CardBackground = "#FFFFFF"
            };
        }

        private static UserRelatedItem MapComment(UserRelatedCommentDto dto)
        {
            var place = !string.IsNullOrWhiteSpace(dto.TaskTitle)
                ? dto.TaskTitle
                : "Без задачи";
            if (!string.IsNullOrWhiteSpace(dto.ProjectName))
            {
                place += $" · {dto.ProjectName}";
            }

            return new UserRelatedItem
            {
                Id = dto.Id,
                RelatedId = dto.TaskId,
                Title = string.IsNullOrWhiteSpace(dto.Content) ? "Пустой комментарий" : dto.Content.Trim(),
                Subtitle = place,
                Badge = "Комментарий",
                BadgeColor = "#00897B",
                Meta = dto.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
                ItemType = "Comment",
                CardBackground = "#FFFFFF"
            };
        }

        private static string MapProjectStatus(int status) => status switch
        {
            0 => "Активный",
            1 => "Завершён",
            2 => "Приостановлен",
            _ => "Неизвестно"
        };
    }
}
