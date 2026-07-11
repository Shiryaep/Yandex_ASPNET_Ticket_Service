# ASP.NET Core Events Service

Это сервис управления событиями на базе ASP.NET Core.

# Table of Contents
1. [Требования, клонирование репозитория и запуск приложения](#requirements)
2. [Настройка базы данных](#setupdb)
3. [Запуск приложения](#start)
4. [Проверка работоспособности](#workcheck)
5. [Фильтрация и пагинация событий](#pagination)
6. [Модели приложения](#models)
7. [Особенности приложения](#features)
8. [Безопасность и права](#auth)

<a name="requirements" id="requirements"></a>
## Требования, клонирование репозитория и запуск приложения

> Смотреть в основном файле README.MD

<a name="setupdb" id="setupdb"></a>
## Настройка базы данных

Пример строки подключения из файла `appsettings.json` для PostgreSQL:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5441;Database=TicketServiceDb;Username=postgres;Password=your_password"
}
```
Учтите, что для сервиса событий база данных разворачивается на порту `5441`.

<a name="start" id="start"></a>
## Запуск приложения

Запустите приложение из директории `YaEvents`:

```bash
dotnet run --project Presentation
```

Сервис пользователей будет доступен по адресу: http://localhost:7011.

<a name="workcheck" id="workcheck"></a>
## Проверка работоспособности

После запуска откройте в браузере:

```bash
https://localhost:7011/swagger
```

Вы должны увидеть интерфейс Swagger UI со списком доступных API‑эндпоинтов и кнопкой `Authorize`.

### Доступные эндпоинты

#### Управление событиями (Events)

    GET /api/events — получить список событий

    GET /api/events/{id} — получить событие по ID

    POST /api/events — создать новое событие

    PUT /api/events/{id} - обновить событие по ID

    DELETE /api/events/{id} - удалить событие по ID

<a name="pagination" id="pagination"></a>
## Фильтрация и пагинация событий

Эндпоинт `GET /api/events` поддерживает следующие параметры запроса:

```
[String]    title - фильтр по заголовку 
[DateTime]  from  - фильтр по дате начала (события, начинающиеся с этой даты или позже)
[DateTime]  to    - фильтр по дате окончания (события, заканчивающиеся до этой даты или раньше)
[Int]       page  - номер страницы (по умолчанию: 1)
[Int]       pageSize - количество событий на странице (по умолчанию: 10)
```

Ответ содержит список событий в формате:

```json
{
  "items": [ ... ],
  "totalCount": 13,
  "page": 1,
  "pageSize": 10
}
```

<a name="models" id="models"></a>
## Модель Event

Событие представлено сущностью `Event` со следующими полями:

```
[Guid]      Id - Уникальный идентификатор события
[String]    Title - Название события
[String?]   Description - Описание
[DateTime]  StartAt - Дата и время начала
[DateTime?] EndAt - Дата и время окончания
[Int]       TotalSeats - Общее количество мест на событие
[Int?]      AvailableSeats - Количество свободных мест
```

Поля `TotalSeats` и `AvailableSeats` используются для управления доступностью мест:
- `TotalSeats` задаётся при создании события и не изменяется.
- `AvailableSeats` уменьшается на 1 при каждом успешном бронировании (метод `TryReserveSeats`) и увеличивается при отмене брони (метод `ReleaseSeats`).

<a name="features" id="features"></a>
## Пример сценария использования

1. **Создание события**  
   ```http
   POST /api/events
   Content-Type: application/json
   Authorization: Bearer .....

   {
     "id": "12345678-1234-1234-1234-123456789abc",
     "title": "Linkin Park",
     "description": "Концерт Linkin Park в Яндекс Музыке",
     "startAt": "2026-03-22T10:30:00",
     "endAt": "2026-03-22T12:00:00",
     "totalSeats": 10
   }
   ```
   Ответ содержит созданное событие с присвоенным `Id`.

2. **Получение списка событий с фильтрацией**  
   ```http
   GET /api/events?title=Linkin&from=2026-03-21&page=1&pageSize=8
   ```

3. **Удаление события**
    ```http
    DELETE /api/events/{eventId}
    Authorization: Bearer .....
    ```

<a name="auth" id="auth"></a>
## Безопасность и права

В проекте реализован эндпоинт для регистрации и входа пользователя. На данный момент есть две роли - User и Admin. В соответствии с этим разграничением некоторые узлы API доступны только определенным категориям пользователей:

**Доступно всем**
```http
   GET /api/events — получить список событий
   GET /api/events/{id} — получить событие по ID
```

**Доступно только администраторам**
```http
   POST /api/events — создать новое событие
   PUT /api/events/{id} - обновить событие по ID
   DELETE /api/events/{id} - удалить событие по ID
```
