FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем все проекты
COPY . .

# Восстанавливаем зависимости для YaUsers.Presentation
RUN dotnet restore "YaUsers/Presentation/YaUsers.Presentation.csproj"

# Публикуем YaBookings.Presentation
RUN dotnet publish "YaUsers/Presentation/YaUsers.Presentation.csproj" \
    -c Debug \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Копируем собранное приложение
COPY --from=build /app/publish .

# Настройки окружения
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

EXPOSE 8080

ENTRYPOINT ["dotnet", "YaUsers.Presentation.dll"]