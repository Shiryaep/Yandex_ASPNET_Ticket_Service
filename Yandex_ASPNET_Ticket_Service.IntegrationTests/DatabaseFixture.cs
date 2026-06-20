using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Yandex_ASPNET_Ticket_Service.IntegrationTests;

/// <summary>
/// Общий fixture для тестов, использующих PostgreSQL контейнер.
/// Реализует IAsyncLifetime для управления жизненным циклом контейнера.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("test_ticket_database")
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

        var context = new AppDbContext(options);
        context.Database.Migrate();
        return context;
    }

    /// <summary>
    /// Сбрасывает базу данных: удаляет все таблицы и заново применяет миграции.
    /// Использует EnsureDeletedAsync() + MigrateAsync() для полного сброса.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();
        await context.Database.CloseConnectionAsync();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }
}