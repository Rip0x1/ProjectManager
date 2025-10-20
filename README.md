# Project Management System

Система управления проектами на .NET 8 с современным WPF-интерфейсом и REST API.

## 🚀 Быстрый запуск

### Установка через Setup
1. **Скачайте** установочные файлы:
   - `ProjectManagementSystem-Setup.exe` - для установки приложения и API

2. **Запустите** установщики в следующем порядке:
   - Сначала API сервер
   - Затем основное приложение

3. **Готово!** Приложение автоматически подключится к API

### Ручная установка (для разработчиков)

#### Требования
- .NET 8.0 SDK
- SQL Server (локальный или Express)
- Windows 10/11

#### Запуск API
```bash
cd ProjectManagementSystem.API
dotnet ef database update
dotnet run
```

#### Запуск приложения
```bash
cd ProjectManagerApp
dotnet run
```

## 📋 Основные возможности

### 👤 Управление пользователями
- Регистрация и авторизация
- Роли: Пользователь, Менеджер, Администратор
- Управление профилями

### 📁 Управление проектами
- Создание и редактирование проектов
- Управление участниками
- Статистика и прогресс

### ✅ Управление задачами
- Создание задач с приоритетами
- Назначение исполнителей
- Отслеживание статусов

### 💬 Комментарии
- Комментарии к задачам и проектам
- Поиск и фильтрация
- Права доступа

### 📊 Dashboard
- Статистика в реальном времени
- Персонализированное приветствие
- Визуальные индикаторы прогресса

## 🏗️ Архитектура

- **API**: ASP.NET Core Web API с Entity Framework Core
- **Клиент**: WPF с Material Design
- **База данных**: SQL Server
- **Паттерн**: MVVM с Dependency Injection

## 📁 Структура проекта

```
ProjectManagementSystem/
├── ProjectManagementSystem.API/     # Web API
├── ProjectManagementSystem.Database/ # База данных
└── ProjectManagerApp/               # WPF клиент
```

## 🔧 Конфигурация

### API
- Порт: `https://localhost:7260`
- База данных: настраивается в `appsettings.json`

### Клиент
- Автоматическое подключение к API
- Настройки в `ApiClient.cs`

## 📞 Поддержка

При возникновении проблем:
1. Проверьте, что API сервер запущен
2. Убедитесь в корректности подключения к базе данных
3. Проверьте логи в консоли приложения

## 🚀 Фото
<img width="1920" height="1080" alt="{558D5A38-45A1-4929-A738-433C0CA5814C}" src="https://github.com/user-attachments/assets/850f740f-a92f-4a38-9225-4afbcca8f6b2" />
<img width="1920" height="1080" alt="{9AA7E10D-4315-469A-8E43-44773FBDA5E1}" src="https://github.com/user-attachments/assets/808ba695-8748-4298-afa2-d0bef02c5002" />
<img width="588" height="795" alt="{FB48D0BA-72C5-4CBB-9262-787A48AF82B7}" src="https://github.com/user-attachments/assets/c55b0ded-3ceb-420d-bdc2-4ecedc254d7a" />


## 📄 Лицензия

См. `LICENSE.txt`
