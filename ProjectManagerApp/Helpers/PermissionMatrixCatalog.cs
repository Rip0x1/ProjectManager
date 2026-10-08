using ProjectManagementSystem.WPF.Models;

namespace ProjectManagementSystem.WPF.Helpers
{
    public static class PermissionMatrixCatalog
    {
        public static IReadOnlyList<PermissionMatrixRow> GetRows() =>
        [
            new PermissionMatrixRow
            {
                Category = "Панель",
                Action = "Просмотр панели",
                Description = "Статистика проектов и задач на главной панели",
                UserAccess = "Да",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Панель",
                Action = "Таблица прав доступа",
                Description = "Просмотр сводки прав по ролям",
                UserAccess = "Да",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Проекты",
                Action = "Просмотр списка проектов",
                Description = "Просмотр всех проектов в системе",
                UserAccess = "Да",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Проекты",
                Action = "Просмотр «Мои проекты»",
                Description = "Проекты, в которых пользователь участвует",
                UserAccess = "Да",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Проекты",
                Action = "Просмотр карточки проекта",
                Description = "Детали проекта, статистика и вложения",
                UserAccess = "Да",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Проекты",
                Action = "Просмотр задач проекта",
                Description = "Список задач внутри карточки проекта",
                UserAccess = "Да",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Проекты",
                Action = "Просмотр комментариев проекта",
                Description = "Комментарии из карточки проекта",
                UserAccess = "Да",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Проекты",
                Action = "Просмотр участников",
                Description = "Список участников из карточки проекта",
                UserAccess = "Да",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Проекты",
                Action = "Создание проекта",
                Description = "Создание нового проекта",
                UserAccess = "Нет",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Проекты",
                Action = "Редактирование проекта",
                Description = "Изменение названия, описания и параметров проекта",
                UserAccess = "Нет",
                ManagerAccess = "Только свой проект",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Проекты",
                Action = "Удаление проекта",
                Description = "Удаление проекта из системы",
                UserAccess = "Нет",
                ManagerAccess = "Только свой проект",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Проекты",
                Action = "Управление участниками",
                Description = "Добавление и удаление участников проекта",
                UserAccess = "Нет",
                ManagerAccess = "Только свой проект",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Проекты",
                Action = "Вложения к проекту",
                Description = "Загрузка, просмотр, скачивание и удаление файлов",
                UserAccess = "Просмотр",
                ManagerAccess = "Полный доступ",
                AdminAccess = "Полный доступ"
            },
            new PermissionMatrixRow
            {
                Category = "Задачи",
                Action = "Просмотр задач",
                Description = "Просмотр списка и деталей задач",
                UserAccess = "Да",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Задачи",
                Action = "Просмотр карточки задачи",
                Description = "Детали задачи, статус, исполнители и вложения",
                UserAccess = "Да",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Задачи",
                Action = "Создание задачи",
                Description = "Создание новых задач",
                UserAccess = "Нет",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Задачи",
                Action = "Редактирование задачи",
                Description = "Изменение задачи",
                UserAccess = "Нет",
                ManagerAccess = "Только свои задачи",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Задачи",
                Action = "Удаление задачи",
                Description = "Удаление задачи",
                UserAccess = "Нет",
                ManagerAccess = "Только свои задачи",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Задачи",
                Action = "Назначение исполнителей",
                Description = "Назначение пользователей на задачи",
                UserAccess = "Нет",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Задачи",
                Action = "Вложения к задаче",
                Description = "Загрузка, просмотр, скачивание и удаление вложений",
                UserAccess = "Просмотр",
                ManagerAccess = "Полный доступ",
                AdminAccess = "Полный доступ"
            },
            new PermissionMatrixRow
            {
                Category = "Комментарии",
                Action = "Просмотр комментариев",
                Description = "Чтение комментариев к задачам и проектам",
                UserAccess = "Да",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Комментарии",
                Action = "Добавление комментария",
                Description = "Комментарии к задачам и проектам",
                UserAccess = "Да",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Комментарии",
                Action = "Удаление комментария",
                Description = "Удаление своих или чужих комментариев",
                UserAccess = "Только свои",
                ManagerAccess = "Свои + в своём проекте",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Комментарии",
                Action = "Вложения к комментарию",
                Description = "Прикрепление файлов к своему комментарию после публикации",
                UserAccess = "Только к своим",
                ManagerAccess = "Только к своим",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Комментарии",
                Action = "Просмотр вложений",
                Description = "Предпросмотр и скачивание файлов в комментариях",
                UserAccess = "Да",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Комментарии",
                Action = "Удаление вложений",
                Description = "Удаление файлов из комментариев",
                UserAccess = "Свои вложения",
                ManagerAccess = "В своём проекте",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Пользователи",
                Action = "Просмотр пользователей",
                Description = "Просмотр списка пользователей системы",
                UserAccess = "Да",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Пользователи",
                Action = "Просмотр карточки пользователя",
                Description = "Детали учётной записи и статистика активности",
                UserAccess = "Да",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Пользователи",
                Action = "Связанные проекты, задачи и комментарии",
                Description = "Списки из карточки пользователя",
                UserAccess = "Да",
                ManagerAccess = "Да",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Пользователи",
                Action = "Добавление в проект",
                Description = "Назначение пользователя участником проекта из карточки",
                UserAccess = "Нет",
                ManagerAccess = "Только свой проект",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Пользователи",
                Action = "Создание пользователя",
                Description = "Регистрация новых пользователей",
                UserAccess = "Нет",
                ManagerAccess = "Нет",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Пользователи",
                Action = "Редактирование пользователя",
                Description = "Изменение данных и роли пользователя",
                UserAccess = "Нет",
                ManagerAccess = "Нет",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Пользователи",
                Action = "Удаление пользователя",
                Description = "Удаление учётной записи",
                UserAccess = "Нет",
                ManagerAccess = "Нет",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Журнал",
                Action = "Вкладка «Логи»",
                Description = "Доступ к журналу действий в главном окне",
                UserAccess = "Нет",
                ManagerAccess = "Нет",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Журнал",
                Action = "Просмотр записей",
                Description = "Список логов, фильтры, поиск и детали записи",
                UserAccess = "Нет",
                ManagerAccess = "Нет",
                AdminAccess = "Да"
            },
            new PermissionMatrixRow
            {
                Category = "Журнал",
                Action = "Очистка журнала",
                Description = "Удаление записей журнала по фильтрам",
                UserAccess = "Нет",
                ManagerAccess = "Нет",
                AdminAccess = "Да"
            }
        ];
    }
}
