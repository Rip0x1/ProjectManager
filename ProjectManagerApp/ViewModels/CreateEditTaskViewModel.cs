using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectManagementSystem.WPF.ViewModels
{
    public partial class CreateEditTaskViewModel : ObservableObject
    {
        private readonly ITasksService _tasksService;
        private readonly IProjectsService _projectsService;
        private readonly IUsersService _usersService;
        private readonly IAuthService _authService;
        private readonly INotificationService _notificationService;
        private readonly IDataRefreshService _dataRefreshService;
        private int? _taskId;

        [ObservableProperty]
        private string _taskTitle = string.Empty;

        [ObservableProperty]
        private string _taskDescription = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ProjectItem> _projects = new();

        [ObservableProperty]
        private ProjectItem _selectedProject;

        [ObservableProperty]
        private ObservableCollection<AssigneeChoice> _assigneeChoices = new();

        [ObservableProperty]
        private int _selectedStatus = 0;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private bool _isEditMode = false;

        [ObservableProperty]
        private int _selectedPriority = 1;

        [ObservableProperty]
        private DateTime? _dueDate;

        [ObservableProperty]
        private DateTime? _completedAt;

        [ObservableProperty]
        private bool _isSaving = false;

        [ObservableProperty]
        private bool _canSave = false;

        [ObservableProperty]
        private ObservableCollection<PendingAttachment> _pendingAttachments = new();

        [ObservableProperty]
        private ObservableCollection<TaskAttachmentItem> _existingAttachments = new();

        public int? TaskId { get; set; }

        public string WindowTitle => TaskId.HasValue ? "Редактирование задачи" : "Создание задачи";
        public string SaveButtonText => TaskId.HasValue ? "Сохранить" : "Создать";

        public event EventHandler<bool>? CloseRequested;

        public CreateEditTaskViewModel(
            ITasksService tasksService,
            IProjectsService projectsService,
            IUsersService usersService,
            IAuthService authService,
            INotificationService notificationService,
            IDataRefreshService dataRefreshService)
        {
            _tasksService = tasksService;
            _projectsService = projectsService;
            _usersService = usersService;
            _authService = authService;
            _notificationService = notificationService;
            _dataRefreshService = dataRefreshService;
        }

        public async Task InitializeForProject(int projectId, string projectName)
        {
            try
            {
                IsLoading = true;
                
                await LoadAsync();
                
                var project = Projects.FirstOrDefault(p => p.Id == projectId);
                if (project != null)
                {
                    SelectedProject = project;
                }
                
                IsEditMode = false;
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка инициализации: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task InitializeForEdit(int taskId)
        {
            try
            {
                IsLoading = true;
                _taskId = taskId;
                TaskId = taskId;
                
                await LoadAsync();
                
                var task = await _tasksService.GetTaskAsync(taskId);
                if (task != null)
                {
                    ApplyTask(task);
                }
                
                IsEditMode = true;
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка инициализации: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadAsync()
        {
            try
            {
                var projectsList = (await _projectsService.GetProjectsAsync()).ToList();
                projectsList.Insert(0, new ProjectItem { Id = 0, Name = "Без проекта" });
                Projects = new ObservableCollection<ProjectItem>(projectsList);

                var usersListDto = await _usersService.GetUsersAsync();
                AssigneeChoices = new ObservableCollection<AssigneeChoice>(
                    usersListDto.Select(u => new AssigneeChoice
                    {
                        Id = u.Id,
                        FullName = $"{u.FirstName} {u.LastName}".Trim()
                    }));

                if (_taskId.HasValue || TaskId.HasValue)
                {
                    var id = _taskId ?? TaskId.Value;
                    var task = await _tasksService.GetTaskAsync(id);
                    if (task != null)
                    {
                        ApplyTask(task);
                    }
                }
                else
                {
                    SelectedProject = Projects.FirstOrDefault();
                }

                ValidateForm();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyTask(TaskDto task)
        {
            TaskTitle = task.Title;
            TaskDescription = task.Description ?? string.Empty;
            SelectedStatus = task.Status;
            SelectedPriority = task.Priority;
            DueDate = ToDatePickerValue(task.DueDate);
            CompletedAt = ToDatePickerValue(task.CompletedAt);

            SelectedProject = Projects.FirstOrDefault(p => p.Id == (task.ProjectId ?? 0)) ?? Projects.FirstOrDefault();

            var selectedIds = task.AssigneeIds ?? new List<int>();
            foreach (var choice in AssigneeChoices)
            {
                choice.IsSelected = selectedIds.Contains(choice.Id);
            }

            ExistingAttachments = new ObservableCollection<TaskAttachmentItem>(task.Attachments ?? new List<TaskAttachmentItem>());
        }

        partial void OnTaskTitleChanged(string value)
        {
            var sanitized = InputValidation.SanitizeTitle(value, ValidationLimits.TaskTitleMax);
            if (sanitized != value)
            {
                TaskTitle = sanitized;
                return;
            }

            ValidateForm();
        }

        partial void OnTaskDescriptionChanged(string value)
        {
            var sanitized = InputValidation.SanitizeMultilineText(value, ValidationLimits.TaskDescriptionMax);
            if (sanitized != value)
            {
                TaskDescription = sanitized;
            }
        }

        private void ValidateForm()
        {
            CanSave = !string.IsNullOrWhiteSpace(TaskTitle) && 
                     TaskTitle.Trim().Length >= 2 && 
                     TaskTitle.Trim().Length <= 100 &&
                     !IsSaving;
        }

        [RelayCommand]
        private void AddAttachment()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Документы и изображения|*.doc;*.docx;*.pdf;*.xls;*.xlsx;*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp|Все файлы|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                if (!AttachmentInputHelper.TryValidateFiles(dialog.FileNames, PendingAttachments.Count, out var error))
                {
                    _notificationService.ShowError(error!);
                    return;
                }

                foreach (var path in dialog.FileNames)
                {
                    PendingAttachments.Add(new PendingAttachment
                    {
                        FilePath = path,
                        FileName = System.IO.Path.GetFileName(path)
                    });
                }
            }
        }

        [RelayCommand]
        private void RemovePendingAttachment(PendingAttachment attachment)
        {
            if (attachment != null)
            {
                PendingAttachments.Remove(attachment);
            }
        }

        [RelayCommand]
        private async Task RemoveExistingAttachmentAsync(TaskAttachmentItem attachment)
        {
            if (attachment == null || !_taskId.HasValue)
            {
                return;
            }

            try
            {
                await _tasksService.DeleteAttachmentAsync(_taskId.Value, attachment.Id);
                ExistingAttachments.Remove(attachment);
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Не удалось удалить вложение: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task PreviewExistingAttachmentAsync(TaskAttachmentItem attachment)
        {
            if (attachment == null || !_taskId.HasValue)
            {
                return;
            }

            try
            {
                var bytes = await _tasksService.DownloadAttachmentAsync(_taskId.Value, attachment.Id);
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
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (!ValidateInput())
                return;

            IsSaving = true;
            CanSave = false;

            try
            {
                var selectedAssigneeIds = AssigneeChoices.Where(a => a.IsSelected).Select(a => a.Id).ToList();
                var projectId = SelectedProject != null && SelectedProject.Id > 0 ? SelectedProject.Id : (int?)null;

                var taskDto = new CreateUpdateTaskDto
                {
                    Title = TaskTitle.Trim(),
                    Description = TaskDescription?.Trim() ?? string.Empty,
                    Status = SelectedStatus,
                    Priority = SelectedPriority,
                    ProjectId = projectId,
                    AuthorId = _authService.CurrentUserId,
                    AssigneeId = selectedAssigneeIds.Count > 0 ? selectedAssigneeIds[0] : null,
                    AssigneeIds = selectedAssigneeIds,
                    DueDate = DueDate?.Date,
                    CompletedAt = CompletedAt?.Date
                };

                int taskId;
                if (_taskId.HasValue)
                {
                    await _tasksService.UpdateTaskAsync(_taskId.Value, taskDto);
                    taskId = _taskId.Value;
                    _notificationService.ShowSuccess("Задача успешно обновлена!");
                }
                else
                {
                    var created = await _tasksService.CreateTaskAsync(taskDto);
                    taskId = created.Id;
                    _notificationService.ShowSuccess("Задача успешно создана!");
                }

                foreach (var pending in PendingAttachments.ToList())
                {
                    await _tasksService.UploadAttachmentAsync(taskId, pending.FilePath, _authService.CurrentUserId);
                }

                _dataRefreshService.Notify(DataRefreshScopes.TaskRelated);
                CloseRequested?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                var userFriendlyMessage = GetUserFriendlyErrorMessage(ex);
                _notificationService.ShowError(userFriendlyMessage);
            }
            finally
            {
                IsSaving = false;
                ValidateForm();
            }
        }

        private bool ValidateInput()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(TaskTitle))
            {
                errors.Add("Введите заголовок задачи");
            }
            else if (TaskTitle.Trim().Length < 2)
            {
                errors.Add("Заголовок задачи должен содержать минимум 2 символа");
            }
            else if (TaskTitle.Trim().Length > 100)
            {
                errors.Add("Заголовок задачи не должен превышать 100 символов");
            }

            if (!string.IsNullOrWhiteSpace(TaskDescription) && TaskDescription.Trim().Length > 1000)
            {
                errors.Add("Описание задачи не должно превышать 1000 символов");
            }

            if (DueDate.HasValue && CompletedAt.HasValue && CompletedAt.Value.Date < DueDate.Value.Date)
            {
                errors.Add("Дата завершения не может быть раньше срока выполнения");
            }

            if (errors.Any())
            {
                _notificationService.ShowError(string.Join("\n", errors));
                return false;
            }

            return true;
        }

        private string GetUserFriendlyErrorMessage(Exception ex)
        {
            var message = ex.Message.ToLower();

            if (message.Contains("заголовок") && message.Contains("уже существует"))
            {
                return "Задача с таким заголовком уже существует";
            }

            if (message.Contains("заголовок") && message.Contains("обязательно"))
            {
                return "Заголовок задачи является обязательным полем";
            }

            if (message.Contains("проект") && message.Contains("не найден"))
            {
                return "Выбранный проект не найден";
            }

            if (message.Contains("исполнитель") && message.Contains("не найден"))
            {
                return "Выбранный исполнитель не найден";
            }

            if (message.Contains("приоритет") && message.Contains("неверный"))
            {
                return "Выберите корректный приоритет задачи";
            }

            if (message.Contains("статус") && message.Contains("неверный"))
            {
                return "Выберите корректный статус задачи";
            }

            if (message.Contains("некорректный запрос"))
            {
                return "Проверьте правильность введенных данных";
            }

            if (message.Contains("ошибка сервера"))
            {
                return "Временная ошибка сервера. Попробуйте позже";
            }

            if (message.Contains("недостаточно прав"))
            {
                return "У вас недостаточно прав для выполнения этой операции";
            }

            return "Произошла ошибка при сохранении задачи. Проверьте введенные данные";
        }

        private static DateTime? ToDatePickerValue(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            var date = value.Value;
            var local = date.Kind == DateTimeKind.Utc ? date.ToLocalTime() : date;
            return local.Date;
        }

    }
}
