# Система управления проектами

Настольный клиент для учёта проектов, задач, участников и комментариев. Серверная часть — ASP.NET Core API и SQL Server. Клиент ставится на рабочие ПК, API и база — в Docker (локально или на сервере).

## Возможности

- Регистрация и вход по логину (не по email).
- Проекты: создание, статусы, руководитель, дедлайн, участники, вложения.
- Задачи: приоритет, статус, сроки, исполнители, вложения.
- Комментарии к задачам и проектам, вложения к комментариям.
- Пользователи и роли: пользователь, менеджер, администратор.
- Журнал аудита действий.
- Дашборд со статистикой.
- Генерация тестовых данных через API.

При первом запуске, если логина `admin` ещё нет, API создаёт администратора `admin` / `admin123`. Если пароль потом сменят, при старте он **не сбрасывается**.

## Структура репозитория

```
ProjectManager-main/
├── ProjectManagerApp/                 WPF-клиент и .sln
├── ProjectManagementSystem.API/       REST API
├── ProjectManagementSystem.Database/  сущности и миграции EF Core
├── Dockerfile
├── docker-compose.yml
├── nuget.config
└── logs/                              живые логи API (появляются после запуска)
```

## Требования

- Windows 10/11 и Visual Studio 2022 (для клиента).
- Docker Desktop с Linux-контейнерами.
- .NET 8 SDK (ставится вместе с VS).

Для обычной работы API из Visual Studio запускать не нужно: он уже в Docker.

## Локальный запуск

### 1. API и база

Из корня репозитория:

```powershell
docker compose up -d --build
```

Контейнеры: `pms-api` и `pms-mssql`. API доступен по адресу:

`http://localhost:7260`

Логи пишутся в `./logs` файлами `api-гггг-мм-дд.txt`. Вложения — в `./uploads`.

Повторный запуск без пересборки:

```powershell
docker compose up -d
```

После изменений в API:

```powershell
docker compose up -d --build
```

Изменения только в WPF Docker не затрагивают — достаточно пересобрать клиент в Visual Studio.

### 2. Клиент

1. Откройте `ProjectManagerApp/ProjectManagerApp.sln`.
2. Стартовый проект — **ProjectManagementSystem.WPF**.
3. Запустите Debug или Release.

Адрес API задаётся в `ProjectManagerApp/appsettings.json`:

```json
{
  "ApiBaseUrl": "http://localhost:7260/api/"
}
```

Для сервера подставьте IP или имя хоста, например `http://10.0.0.12:7260/api/`. Файл копируется в каталог сборки.

Не запускайте API из Visual Studio параллельно с Docker: профиль `https` тоже занимает порт `7260`.

### 3. Вход

Логин: `admin`  
Пароль: `admin123`

## Генерация данных

Эндпоинты только `POST`. В браузере открывать бессмысленно. Каждый вызов **очищает базу** и заново создаёт `admin` / `admin123`.

| Набор | URL |
| --- | --- |
| Малый (~100) | `http://localhost:7260/api/DataGenerator/generate-small` |
| Средний (~1 000) | `http://localhost:7260/api/DataGenerator/generate-medium` |
| 10 000 | `http://localhost:7260/api/DataGenerator/generate-10k` |
| 20 000 | `http://localhost:7260/api/DataGenerator/generate-20k` |
| 50 000 | `http://localhost:7260/api/DataGenerator/generate-50k` |
| 100 000 | `http://localhost:7260/api/DataGenerator/generate-100k` |

Пример:

```powershell
Invoke-RestMethod -Method Post -Uri "http://localhost:7260/api/DataGenerator/generate-medium"
```

## API

Базовый URL: `http://localhost:7260/api/`

| Контроллер | Назначение |
| --- | --- |
| `Auth` | регистрация, вход, выход |
| `Users` | пользователи |
| `Projects` | проекты |
| `ProjectMembers` / `ProjectUsers` | участники проекта |
| `Tasks` | задачи |
| `Comments` | комментарии |
| `AuditLogs` | журнал аудита |
| `Statistics` | статистика дашборда |
| `DataGenerator` | тестовые наборы |

## Роли

| Значение | Роль |
| --- | --- |
| 0 | Пользователь |
| 1 | Менеджер |
| 2 | Администратор |

Права на разделы и действия задаются в клиенте (матрица прав). Администратор управляет пользователями и видит журнал.

## База данных

Контейнер SQL Server хранит данные в томе `mssql_data`. При старте API применяет миграции EF Core.

Полностью сбросить базу (том удалится, останется только новый `admin`):

```powershell
docker compose down -v
docker compose up -d
```

`docker compose down` без `-v` данные сохраняет.

Локальный `appsettings.json` API с LocalDB (`TaskMasterDB2`) для Docker-сценария не используется: строка подключения задаётся в `docker-compose.yml`.
