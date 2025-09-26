## Project Management System

Учебный проект на русском языке: ASP.NET Core Web API + WPF (MaterialDesignInXaml) для управления проектами, задачами и комментариями.

### Состав репозитория
- **ProjectManagementSystem.API** — Web API на .NET 8 (EF Core)
- **ProjectManagementSystem.Database** — сущности и DbContext
- **ProjectManagerApp** — WPF‑клиент на .NET 8 с Material Design

### Требования
- **.NET SDK** 8.0+
- **Windows 10/11** (для WPF‑клиента)

### Быстрый старт
#### 1) Запуск API
1. Откройте решение в Visual Studio или через `dotnet` CLI
2. Выберите проект запуска: `ProjectManagementSystem.API`
3. Проверьте HTTPS профиль (по умолчанию `https://localhost:7260`)
4. Запустите API (F5 или `dotnet run` в каталоге API)

**Контроллеры**:
- `api/auth/login`, `api/auth/register`
- `api/users`
- `api/projects`
- `api/tasks`
- `api/comments`

#### 2) Запуск WPF‑клиента
1. Убедитесь, что API доступен по адресу, указанному в `ProjectManagerApp/Services/ApiClient.cs` (`BaseUrl`)
2. Запустите проект `ProjectManagerApp`
3. В окне входа доступны:
   - Вход (email/пароль)
   - Регистрация с возвратом на вход
   - Показать/скрыть пароль (иконка глаза)
   - Подсказки: **зелёная** — успех, **красная** — ошибка

### Архитектура клиента
- **DI**: `Microsoft.Extensions.Hosting` (см. `App.xaml.cs`)
- **Сервисы**:
  - `ApiClient` — обёртка над `HttpClient`
  - `AuthService` — вход/регистрация, текущий пользователь
  - `NavigationService` — навигация (`LoginView` → `MainWindow`)
  - `NotificationService` — Material Snackbar
- **MVVM**: ViewModels (`Login`, `Dashboard`, `Projects`, `Tasks`, `Users`) и соответствующие Views
- **Главное окно**: вкладки «Панель», «Проекты», «Задачи», «Пользователи»

### Конфигурация
- **BaseUrl**: `ProjectManagerApp/Services/ApiClient.cs`
- В dev‑режиме отключена строгая проверка сертификата для локального HTTPS

### Полезные заметки
- 401 при входе отображается как: **«Неверный email или пароль»**
- Дубликат email при регистрации: **«Аккаунт с таким email уже существует»**
- После входа — открывается главное окно во весь экран, окно входа закрывается
- После регистрации — возврат на форму входа с подставленным email

### Планы
- CRUD‑интерфейсы для проектов, задач, комментариев
- Фильтры/поиск, изменение статусов задач
- Роли и ограничения доступа
- Дашборд с аналитикой

### Лицензия
См. `LICENSE.txt`.