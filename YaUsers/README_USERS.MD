# ASP.NET Core Users Service

Это сервис управления пользователями на базе ASP.NET Core.

# Table of Contents
1. [Требования, клонирование репозитория и запуск приложения](#requirements)
2. [Настройка базы данных](#setupdb)
3. [Запуск приложения](#start)
4. [Проверка работоспособности](#workcheck)
5. [Модели приложения](#models)
6. [Особенности приложения](#features)
7. [Безопасность и права](#auth)

<a name="requirements" id="requirements"></a>
## Требования, клонирование репозитория и запуск приложения

> Смотреть в основном файле README.MD

<a name="setupdb" id="setupdb"></a>
## Настройка базы данных

Пример строки подключения из файла `appsettings.json` для PostgreSQL:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5443;Database=TicketServiceDb;Username=postgres;Password=your_password"
}
```
Учтите, что для сервиса пользователей база данных разворачивается на порту `5443`.

<a name="start" id="start"></a>
## Запуск приложения

Запустите приложение из директории `YaUsers`:

```bash
dotnet run --project Presentation
```

Сервис пользователей будет доступен по адресу: http://localhost:7013.

<a name="workcheck" id="workcheck"></a>
## Проверка работоспособности

После запуска откройте в браузере:

```bash
https://localhost:7013/swagger
```

Вы должны увидеть интерфейс Swagger UI со списком доступных API‑эндпоинтов и кнопкой `Authorize`.

### Доступные эндпоинты

#### Авторизация (Auth)

    POST /api/Auth/register - зарегистрироваться в приложении

    POST /api/Auth/login - войти в приложение

<a name="models" id="models"></a>
## Модель User

Пользователь представлен сущностью `User` со следующими полями:

```
[Guid]      Id - Уникальный идентификатор пользователя
[String]    Login - Имя пользователя для входа
[String]    PasswordHash - Хэш пользовательского пароля
[UserRoles] Role - Роль пользователя - в данном случае User или Admin
```

<a name="features" id="features"></a>
## Пример сценария использования

1. **Регистрация пользователя**  
   ```http
   POST /api/Auth/register
   Content-Type: application/json

   {
     "login": "Login",
     "password": "Password",
     "role": "0"
   }
   ```

2. **Получение токена**  
   ```http
   POST /api/Auth/login
   Content-Type: application/json

   {
     "login": "Login",
     "password": "Password"
   }
   ```

<a name="auth" id="auth"></a>
## Безопасность и права

В проекте реализован эндпоинт для регистрации и входа пользователя. На данный момент есть две роли - User и Admin. В соответствии с этим разграничением некоторые узлы API доступны только определенным категориям пользователей:

**Доступно всем**
```http
   POST /api/Auth/register - зарегистрироваться в приложении
   POST /api/Auth/login - войти в приложение
```
