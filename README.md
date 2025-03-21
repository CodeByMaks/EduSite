# EduSite

Веб-приложение для образовательных целей, разработанное на ASP.NET Core 8.0.

## Описание

EduSite - это современное веб-приложение, построенное с использованием ASP.NET Core MVC. Проект использует Entity Framework Core для работы с базой данных и включает в себя систему аутентификации пользователей.

## Технологии

- ASP.NET Core 8.0
- Entity Framework Core 9.0
- SQLite
- ASP.NET Core Identity
- MVC (Model-View-Controller)

## Структура проекта

```
EduSite/
├── Controllers/     # Контроллеры приложения
├── Models/         # Модели данных
├── Views/          # Представления
├── Data/           # Контекст базы данных и миграции
├── wwwroot/        # Статические файлы (CSS, JavaScript, изображения)
└── Migrations/     # Миграции базы данных
```

## Требования

- .NET 8.0 SDK
- Visual Studio 2022 или JetBrains Rider
- SQLite

## Установка и запуск

1. Клонируйте репозиторий:
```bash
git clone [URL репозитория]
```

2. Перейдите в директорию проекта:
```bash
cd EduSite
```

3. Восстановите зависимости:
```bash
dotnet restore
```

4. Примените миграции базы данных:
```bash
dotnet ef database update
```

5. Запустите приложение:
```bash
dotnet run
```

Приложение будет доступно по адресу: https://localhost:5001

## Лицензия

Этот проект распространяется под лицензией, указанной в файле LICENSE. 