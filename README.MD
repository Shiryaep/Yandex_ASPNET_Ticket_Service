# ASP.NET Core Web API

Это веб‑API на базе ASP.NET Core для бронирования билетов на мероприятие.

## Требования

Перед запуском убедитесь, что у вас установлены:

- [.NET SDK 10.0 или выше](https://dotnet.microsoft.com/download)
- [Visual Studio 2026](https://visualstudio.microsoft.com/) или [Visual Studio Code](https://code.visualstudio.com/)

## Клонирование репозитория

1. Откройте терминал или командную строку.
2. Выполните команду для клонирования репозитория:

   ```bash
   git clone https://github.com/Shiryaep/Yandex_ASPNET_Ticket_Service.git
   cd .\Yandex_ASPNET_Ticket_Service\
   ```
3. Выполните команду для восстановления зависимостей:

    ```bash
    dotnet restore
    ```
4. Соберите проект с помощью команды:

    ```bash
    dotnet build
    ```


## Запуск приложения
### Способ 1. Через командную строку

Запустите приложение находясь в папке проекта:

```bash
dotnet run
```

Приложение будет доступно по адресу: https://localhost:5062 или http://localhost:7011.

### Способ 2. Через Visual Studio

 - Откройте решение (.sln файл) в Visual Studio.

 - Нажмите F5 или Ctrl+F5 для запуска.

### Способ 3. Через Visual Studio Code

 - Установите расширение C# от Microsoft.

 - Откройте папку проекта в VS Code.

 - Используйте встроенный терминал для выполнения команды dotnet run.



## Проверка работоспособности

После запуска откройте в браузере:

```bash
https://localhost:7011/swagger
```

Вы должны увидеть интерфейс Swagger UI со списком доступных API‑эндпоинтов.

### Доступные эндпоинты

    GET /events — получить список событий

    GET /events/{id} — получить событие по ID

    POST /events — создать новое событие

    PUT /events/{id} - обновить событие по ID

    DELETE /events/{id} - удалить событие по ID

## Обновление для API

 - Появились фильтры
    ```
    GET /events — получить список событий с фильтрами:
    
    [String]    title - по заголовку
    [DateTime]  from - по дате начала
    [DateTime]  to - по дате окончания
    [Int]       page - текущая страница (default: 1)
    [Int]       pageSize - событий на странице (default: 10)
    ```

 - Появились тесты
    
    Запуск тестов командой в папке проекта 
    ```
    dotnet test
    ```

## Формат ошибок
API использует формат ошибок [Problem Details (RFC 7807)](https://datatracker.ietf.org/doc/html/rfc7807)