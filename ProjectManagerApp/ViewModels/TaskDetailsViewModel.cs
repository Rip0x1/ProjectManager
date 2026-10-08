using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ProjectManagementSystem.WPF.ViewModels
{
    public partial class TaskDetailsViewModel : ObservableObject
    {
        private readonly ITasksService _tasksService;
        private readonly IAuthService _authService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;

        [ObservableProperty]
        private TaskItem _currentTask = new();

        [ObservableProperty]
        private ObservableCollection<TaskAttachmentItem> _attachments = new();

        [ObservableProperty]
        private bool _isBusy;

        public bool HasAttachments => Attachments.Count > 0;

        public bool CanEditTask => _permissionService.CanEditTask(CurrentTask.AuthorId);

        public TaskDetailsViewModel(
            ITasksService tasksService,
            IAuthService authService,
            INotificationService notificationService,
            IPermissionService permissionService)
        {
            _tasksService = tasksService;
            _authService = authService;
            _notificationService = notificationService;
            _permissionService = permissionService;
        }

        public async Task InitializeAsync(TaskItem task)
        {
            try
            {
                IsBusy = true;
                var dto = await _tasksService.GetTaskAsync(task.Id);
                CurrentTask = TasksService.MapToTaskItem(dto);
            }
            catch
            {
                CurrentTask = task;
            }
            finally
            {
                IsBusy = false;
            }

            OnPropertyChanged(nameof(CanEditTask));
            await ReloadAttachmentsAsync();
        }

        partial void OnCurrentTaskChanged(TaskItem value)
        {
            OnPropertyChanged(nameof(CanEditTask));
        }

        [RelayCommand]
        private async Task EditTaskAsync()
        {
            if (!CanEditTask)
            {
                _notificationService.ShowWarning("У вас нет прав для редактирования этой задачи");
                return;
            }

            try
            {
                var window = new Views.CreateEditTaskWindow
                {
                    Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                        ?? Application.Current.MainWindow
                };

                await window.PrepareAsync(CurrentTask.Id);

                if (window.ShowDialog() == true)
                {
                    await InitializeAsync(CurrentTask);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка редактирования задачи: {ex.Message}");
            }
        }

        partial void OnAttachmentsChanged(ObservableCollection<TaskAttachmentItem> value)
        {
            OnPropertyChanged(nameof(HasAttachments));
        }

        private async System.Threading.Tasks.Task ReloadAttachmentsAsync()
        {
            var items = await _tasksService.GetAttachmentsAsync(CurrentTask.Id);
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
                    await _tasksService.UploadAttachmentAsync(CurrentTask.Id, path, _authService.CurrentUserId);
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
                var bytes = await _tasksService.DownloadAttachmentAsync(CurrentTask.Id, attachment.Id);
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
                var bytes = await _tasksService.DownloadAttachmentAsync(CurrentTask.Id, attachment.Id);
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
                await _tasksService.DeleteAttachmentAsync(CurrentTask.Id, attachment.Id);
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
