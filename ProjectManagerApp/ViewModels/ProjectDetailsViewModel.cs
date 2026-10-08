using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using ProjectManagerApp.Services;
using ProjectManagerApp.ViewModels;
using ProjectManagerApp.Views;
using ProjectManagementSystem.WPF;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ProjectManagementSystem.WPF.ViewModels
{
    public partial class ProjectDetailsViewModel : ObservableObject
    {
        private readonly IProjectsService _projectsService;
        private readonly ITasksService _tasksService;
        private readonly IUsersService _usersService;
        private readonly IAuthService _authService;
        private readonly INotificationService _notificationService;
        private readonly IApiClient _apiClient;
        private readonly IDataRefreshService _dataRefreshService;

        [ObservableProperty]
        private ProjectItem _currentProject = new();

        [ObservableProperty]
        private ObservableCollection<TaskAttachmentItem> _attachments = new();

        [ObservableProperty]
        private bool _isBusy;

        public bool HasAttachments => Attachments.Count > 0;

        public ProjectDetailsViewModel(
            IProjectsService projectsService,
            ITasksService tasksService,
            IUsersService usersService,
            IAuthService authService,
            INotificationService notificationService,
            IApiClient apiClient,
            IDataRefreshService dataRefreshService)
        {
            _projectsService = projectsService;
            _tasksService = tasksService;
            _usersService = usersService;
            _authService = authService;
            _notificationService = notificationService;
            _apiClient = apiClient;
            _dataRefreshService = dataRefreshService;

            _dataRefreshService.Changed += OnDataRefresh;
        }

        public async Task InitializeAsync(ProjectItem project)
        {
            await ReloadProjectAsync(project);
            await ReloadAttachmentsAsync();
        }

        private void OnDataRefresh(object? sender, DataRefreshScope scope)
        {
            if (!scope.HasFlag(DataRefreshScope.Projects) && !scope.HasFlag(DataRefreshScope.MyProjects))
            {
                return;
            }

            Application.Current?.Dispatcher.BeginInvoke(async () =>
            {
                await ReloadProjectAsync(CurrentProject);
            });
        }

        private async System.Threading.Tasks.Task ReloadProjectAsync(ProjectItem fallback)
        {
            try
            {
                var dto = await _projectsService.GetProjectAsync(fallback.Id);
                CurrentProject = new ProjectItem
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Description = dto.Description ?? string.Empty,
                    ManagerId = dto.ManagerId,
                    CreatedAt = dto.CreatedAt,
                    Deadline = dto.Deadline,
                    ManagerName = dto.Manager != null ? $"{dto.Manager.FirstName} {dto.Manager.LastName}" : "—",
                    Status = dto.Status,
                    ParticipantsCount = dto.ParticipantsCount,
                    TasksCount = dto.TasksCount,
                    CommentsCount = dto.CommentsCount
                };
            }
            catch
            {
                CurrentProject = fallback;
            }
        }

        [RelayCommand]
        private async Task ViewMembersAsync()
        {
            try
            {
                var window = new ProjectMembersViewWindow();
                var viewModel = App.GetService<ProjectMembersViewViewModel>();
                await viewModel.InitializeAsync(CurrentProject.Id, CurrentProject.Name);
                window.DataContext = viewModel;
                window.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                    ?? Application.Current.MainWindow;
                WindowFitHelper.ApplySizeBeforeShow(window, 920, 680);
                window.ShowDialog();
                await ReloadProjectAsync(CurrentProject);
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка открытия участников проекта: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ViewCommentsAsync()
        {
            try
            {
                var window = new ProjectCommentsViewWindow();
                var viewModel = App.GetService<ProjectCommentsViewViewModel>();
                await viewModel.Initialize(CurrentProject.Id, CurrentProject.Name, CurrentProject.ManagerId);
                window.DataContext = viewModel;
                window.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                    ?? Application.Current.MainWindow;
                WindowFitHelper.ApplySizeBeforeShow(window, 920, 680);
                window.ShowDialog();
                await ReloadProjectAsync(CurrentProject);
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка открытия комментариев: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ViewTasksAsync()
        {
            try
            {
                var window = new ProjectTasksWindow();
                var viewModel = new ProjectTasksViewModel(
                    _tasksService,
                    _projectsService,
                    _usersService,
                    _authService,
                    _notificationService,
                    _apiClient);

                await viewModel.Initialize(CurrentProject.Id, CurrentProject.Name);
                window.DataContext = viewModel;
                window.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                    ?? Application.Current.MainWindow;
                WindowFitHelper.ApplySizeBeforeShow(window, 920, 740);
                window.ShowDialog();
                await ReloadProjectAsync(CurrentProject);
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка открытия задач проекта: {ex.Message}");
            }
        }

        partial void OnAttachmentsChanged(ObservableCollection<TaskAttachmentItem> value)
        {
            OnPropertyChanged(nameof(HasAttachments));
        }

        private async System.Threading.Tasks.Task ReloadAttachmentsAsync()
        {
            var items = await _projectsService.GetAttachmentsAsync(CurrentProject.Id);
            Attachments = new ObservableCollection<TaskAttachmentItem>(items);
            OnPropertyChanged(nameof(HasAttachments));
        }

        [RelayCommand]
        private async Task AddAttachmentAsync()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Документы и изображения|*.doc;*.docx;*.pdf;*.xls;*.xlsx;*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                IsBusy = true;
                foreach (var path in dialog.FileNames)
                {
                    await _projectsService.UploadAttachmentAsync(CurrentProject.Id, path, _authService.CurrentUserId);
                }
                await ReloadAttachmentsAsync();
                _notificationService.ShowSuccess("Вложения добавлены");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Не удалось прикрепить файл: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task PreviewAttachmentAsync(TaskAttachmentItem attachment)
        {
            if (attachment == null)
            {
                return;
            }

            try
            {
                IsBusy = true;
                var bytes = await _projectsService.DownloadAttachmentAsync(CurrentProject.Id, attachment.Id);
                var owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                    ?? Application.Current.MainWindow;
                if (owner != null)
                {
                    AttachmentPreviewHelper.ShowPreview(owner, attachment.FileName, bytes, attachment.ContentType);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Не удалось открыть предпросмотр: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DownloadAttachmentAsync(TaskAttachmentItem attachment)
        {
            if (attachment == null)
            {
                return;
            }

            var dialog = new SaveFileDialog
            {
                FileName = attachment.FileName,
                Filter = "Все файлы|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                IsBusy = true;
                var bytes = await _projectsService.DownloadAttachmentAsync(CurrentProject.Id, attachment.Id);
                await File.WriteAllBytesAsync(dialog.FileName, bytes);
                Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Не удалось скачать файл: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteAttachmentAsync(TaskAttachmentItem attachment)
        {
            if (attachment == null)
            {
                return;
            }

            try
            {
                IsBusy = true;
                await _projectsService.DeleteAttachmentAsync(CurrentProject.Id, attachment.Id);
                Attachments.Remove(attachment);
                OnPropertyChanged(nameof(HasAttachments));
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Не удалось удалить вложение: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
