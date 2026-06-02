# StudyTracker

StudyTracker — это ASP.NET Core MVC-приложение для организации курсов, отслеживания учебных задач, управления сроками, назначения студентов на курсы и экспорта отчетов о прогрессе.

## Роли

- `Administrator`: создает, редактирует и удаляет курсы и задачи; назначает курсы студентам; экспортирует список студентов с просроченными задачами.
- `Student`: видит только назначенные курсы; просматривает задачи по назначенным курсам; изменяет только собственный статус задачи; экспортирует задачи для назначенного курса.

## Начальные учетные записи

Все начальные учетные записи используют пароль `Study123!`.

| Роль | Email |
| --- | --- |
| Administrator | `admin@studytracker.local` |
| Student | `student@studytracker.local` |
| Student | `ivan@studytracker.local` |

## Основные возможности

- CRUD-операции для курсов.
- CRUD-операции для учебных задач внутри курсов.
- Индивидуальный статус задачи для каждого студента через `StudentTaskStatus`, поэтому изменение статуса одним студентом не влияет на других студентов.
- Фильтрация задач по статусу.
- Сортировка задач по сроку выполнения по возрастанию или убыванию.
- Визуальное выделение просроченных задач.
- Авторизация на основе ролей с помощью ASP.NET Core Identity.
- База данных PostgreSQL через миграции Entity Framework Core Code First.
- Экспорт отчетов в DOCX и XLSX.
- Razor UI на основе Bootstrap.

## Пакеты NuGet

Проект использует:

- `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.AspNetCore.Identity.UI`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Tools`
- `Npgsql.EntityFrameworkCore.PostgreSQL`

SQLite-пакет нужно удалить, если он установлен:

```powershell
dotnet remove package Microsoft.EntityFrameworkCore.Sqlite
```

Пакеты для PostgreSQL и миграций:

```powershell
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

Отчеты DOCX и XLSX генерируются как минимальные файлы Open XML с использованием встроенных .NET API для ZIP/XML, поэтому дополнительные пакеты для экспорта не требуются.

## Локальный запуск

Установите PostgreSQL и создайте базу данных одним из способов:

```powershell
psql -U postgres -f .\Database\00_create_database.sql
```

Или используйте EF Core migrations после настройки строки подключения.

В `appsettings.json` укажите пароль PostgreSQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=study_tracker_db;Username=postgres;Password=your_password"
  }
}
```

```powershell
dotnet restore
dotnet build
dotnet tool install --tool-path .\.tools dotnet-ef --version 9.0.1
.\.tools\dotnet-ef database update
dotnet run
```

Приложение также автоматически выполняет миграции и добавляет начальные данные при запуске.

Для ручного создания схемы вместо миграций выполните скрипты в базе `study_tracker_db`:

```powershell
psql -U postgres -d study_tracker_db -f .\Database\01_create_schema.sql
psql -U postgres -d study_tracker_db -f .\Database\02_seed_data.sql
```

Не смешивайте ручное создание таблиц и `dotnet ef database update` для одной пустой базы.

## Docker

Если PostgreSQL запущен на хост-машине через Docker Desktop для Windows/macOS, используйте `host.docker.internal`:

```powershell
docker build -t studytracker .
docker run --rm -p 8080:8080 `
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=study_tracker_db;Username=postgres;Password=your_password" `
  studytracker
```

Если PostgreSQL запущен в отдельном контейнере, поместите оба контейнера в одну Docker-сеть и укажите имя PostgreSQL-контейнера как `Host`:

```powershell
docker network create studytracker-net
docker run --name studytracker-postgres --network studytracker-net `
  -e POSTGRES_DB=study_tracker_db `
  -e POSTGRES_USER=postgres `
  -e POSTGRES_PASSWORD=postgres `
  -p 5432:5432 `
  -d postgres:16
docker logs -f studytracker-postgres
```

Дождитесь сообщения `database system is ready to accept connections`, затем остановите просмотр логов через `Ctrl+C` и запустите приложение:

```powershell
docker build -t studytracker .
docker run --rm --network studytracker-net -p 8080:8080 `
  -e ConnectionStrings__DefaultConnection="Host=studytracker-postgres;Port=5432;Database=study_tracker_db;Username=postgres;Password=your_password" `
  studytracker
```

Затем откройте `http://localhost:8080`.
