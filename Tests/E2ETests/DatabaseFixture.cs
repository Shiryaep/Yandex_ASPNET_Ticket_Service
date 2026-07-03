using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace E2ETests;

/// <summary>
/// Общий fixture для тестов, использующих PostgreSQL контейнер.
/// Реализует IAsyncLifetime для управления жизненным циклом контейнера.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("test_ticket_database_e2e")
        .Build();

    /// <summary>
    /// Строка подключения к контейнеру PostgreSQL.
    /// </summary>
    public string ConnectionString => _postgreSqlContainer.GetConnectionString();

    /// <summary>
    /// Запускает контейнер PostgreSQL.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        // При первом запуске контейнера сразу применяем миграции, 
        // чтобы создать схему БД. Дальше мы будем только очищать данные.
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Останавливает и удаляет контейнер PostgreSQL.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    /// <summary>
    /// Создает новый экземпляр AppDbContext с подключением к тестовой БД.
    /// Автоматически применяет миграции.
    /// </summary>
    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        //var context = new AppDbContext(options);
        //context.Database.Migrate();
        return new AppDbContext(options);
    }

    /// <summary>
    /// Сбрасывает базу данных: удаляет все таблицы и заново применяет миграции.
    /// Использует EnsureDeletedAsync() + MigrateAsync() для полного сброса.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();

        // Получаем список всех таблиц из модели EF Core
        var tables = context.Model.GetEntityTypes()
            .Select(t => t.GetTableName())
            .Where(t => !string.IsNullOrEmpty(t))
            .Distinct()
            .ToList();

        // Очищаем каждую таблицу. 
        // CASCADE удалит связанные записи, RESTART IDENTITY сбросит счетчики (автоинкремент)
        foreach (var table in tables)
        {
            await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{table}\" RESTART IDENTITY CASCADE;");
        }
    }
}