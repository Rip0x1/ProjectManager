namespace ProjectManagementSystem.WPF.Helpers
{
    public static class InputFieldHints
    {
        public const string PersonName =
            "Только буквы, пробел, дефис и апостроф. От 2 до 50 символов.";

        public const string Login =
            "Логин: буквы, цифры, точка, дефис и подчёркивание. От 3 до 100 символов, без пробелов.";

        public const string Password =
            "От 6 до 100 символов. Без управляющих символов.";

        public const string ConfirmPassword =
            "Повторите пароль — он должен совпадать с полем «Пароль».";

        public const string ProjectName =
            "Название проекта: от 2 до 100 символов.";

        public const string ProjectDescription =
            "Краткое описание проекта. До 500 символов.";

        public const string TaskTitle =
            "Название задачи: от 2 до 100 символов.";

        public const string TaskDescription =
            "Описание задачи. До 1000 символов.";

        public const string Comment =
            "Текст комментария. До 1000 символов. Можно приложить файлы.";

        public const string Search =
            "Поиск по тексту и ID. До 200 символов.";

        public const string PageNumber =
            "Номер страницы — только целое число.";

        public const string ProjectDeadline =
            "Дата не может быть в прошлом.";
    }
}
