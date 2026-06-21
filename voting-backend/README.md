# VotingApp Backend

ASP.NET Core 8 Web API для сервиса онлайн-голосований.

## Технологии

- ASP.NET Core 8
- Entity Framework Core 8 (Code First)
- MS SQL Server (Docker/Podman)
- JWT Authentication
- SignalR (WebSockets / SSE fallback)
- Swagger UI

## Структура проекта

```
VotingApp/
├── Controllers/        # REST API контроллеры
│   ├── AuthController.cs
│   ├── AdminController.cs
│   ├── PollsController.cs
│   └── VotesController.cs
├── Hubs/               # SignalR хабы
│   └── PollHub.cs
├── Models/             # EF Core сущности
│   ├── User.cs
│   ├── Poll.cs
│   ├── PollOption.cs
│   ├── Vote.cs
│   └── RefreshToken.cs
├── Data/               # DbContext и миграции
│   ├── AppDbContext.cs
│   └── Migrations/
├── DTOs/               # Data Transfer Objects
│   ├── Auth/
│   ├── Poll/
│   └── Vote/
├── Repositories/       # Repository layer
├── Services/           # Business logic layer
├── appsettings.json
└── Program.cs
```

## Запуск на Fedora (Linux)

### 1. Предварительные требования

Установлены .NET 8 SDK и Podman (или Docker).

Если .NET не установлен:
```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --install-dir "$HOME/.dotnet"
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"
```

Установка `dotnet-ef`:
```bash
dotnet tool install --global dotnet-ef --version 8.0.4
```

### 2. Запуск MS SQL Server через Podman

```bash
podman run -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=YourStrong@Pass1" \
  -p 1433:1433 --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Проверка:
```bash
podman ps
```

### 3. Применение миграций

```bash
cd voting-backend/VotingApp
dotnet ef database update
```

### 4. Запуск приложения

```bash
dotnet run
```

API будет доступен по адресам:
- Swagger UI: `https://localhost:5001/swagger/index.html`
- API: `https://localhost:5001/api/...`
- SignalR Hub: `wss://localhost:5001/hubs/poll`

## API Endpoints

### Auth
- `POST /api/auth/register` — регистрация
- `POST /api/auth/login` — вход
- `POST /api/auth/refresh` — обновление токенов
- `GET /api/auth/me` — текущий пользователь

### Polls (публичные)
- `GET /api/polls` — список активных голосований
- `GET /api/polls/{id}` — детали голосования

### Votes
- `POST /api/polls/{pollId}/vote` — проголосовать
- `GET /api/polls/{pollId}/results` — результаты
- `GET /api/polls/{pollId}/my-vote` — мой голос

### Admin
- `POST /api/admin/polls` — создать голосование (draft)
- `GET /api/admin/polls` — все голосования
- `PUT /api/admin/polls/{id}` — редактировать (только draft)
- `POST /api/admin/polls/{id}/publish` — опубликовать
- `DELETE /api/admin/polls/{id}` — удалить

## Создание администратора

Первый зарегистрировавшийся пользователь автоматически становится администратором.

Для назначения последующих администраторов используйте панель администратора или endpoint `POST /api/admin/users/make-admin`.

## Демонстрация с большим количеством голосов

Для показа работы с сотнями голосов рекомендуется:

1. Создать голосование через Swagger UI (`/swagger`)
2. Опубликовать его через `POST /api/admin/polls/{id}/publish`
3. Выполнить несколько голосований вручную через Swagger
4. При открытии страницы голосования на фронтенде результаты будут обновляться в реальном времени (SignalR)

В будущем можно добавить seed-скрипт для автоматического заполнения тестовыми данными.
