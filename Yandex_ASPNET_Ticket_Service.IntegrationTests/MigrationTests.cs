using Microsoft.EntityFrameworkCore;
using Yandex_ASPNET_Ticket_Service.DataAccess;

namespace Yandex_ASPNET_Ticket_Service.IntegrationTests;

/// <summary>
/// Тесты, проверяющие корректность применения миграций:
/// наличие таблиц, внешних ключей и ограничений.
/// </summary>

[Collection("DatabaseCollection")]
public class MigrationTests
{
    private readonly DatabaseFixture _fixture;

    public MigrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// Проверяет, что после применения миграций существуют таблицы Events и Bookings.
    /// </summary>
    [Fact]
    public async Task Migrations_ShouldCreateEventsAndBookingsTables()
    {
        // Arrange
        await using var context = _fixture.CreateContext();

        // Act
        var eventsTableExists = await TableExistsAsync(context, "Events");
        var bookingsTableExists = await TableExistsAsync(context, "Bookings");

        // Assert
        Assert.True(eventsTableExists, "Таблица 'Events' должна существовать после миграций.");
        Assert.True(bookingsTableExists, "Таблица 'Bookings' должна существовать после миграций.");
    }

    /// <summary>
    /// Проверяет, что внешний ключ bookings.event_id → events.id создан и работает.
    /// </summary>
    [Fact]
    public async Task Migrations_ShouldCreateForeignKeyFromBookingsToEvents()
    {
        // Arrange
        await using var context = _fixture.CreateContext();

        // Act
        var foreignKeyExists = await ForeignKeyExistsAsync(context, "Bookings", "EventId", "Events", "Id");

        // Assert
        Assert.True(foreignKeyExists, "Внешний ключ от Bookings.EventId к Events.Id должен существовать.");
    }

    /// <summary>
    /// Проверяет, что можно создать Event и связанный с ним Booking,
    /// что подтверждает работоспособность внешнего ключа.
    /// </summary>
    [Fact]
    public async Task ForeignKey_ShouldAllowLinkingBookingToEvent()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();

        var eventEntity = Models.Event.Create(
            "Test Event",
            "Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            100);

        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        // Act
        var booking = Models.Booking.CreatePending(eventEntity.Id);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        // Assert
        var savedBooking = await context.Bookings
            .Include(b => b.Event)
            .FirstOrDefaultAsync(b => b.Id == booking.Id);

        Assert.NotNull(savedBooking);
        Assert.NotNull(savedBooking.Event);
        Assert.Equal(eventEntity.Id, savedBooking.Event.Id);
    }

    /// <summary>
    /// Проверяет, что попытка создать Booking с несуществующим EventId
    /// приводит к нарушению внешнего ключа (исключение DbUpdateException).
    /// </summary>
    [Fact]
    public async Task ForeignKey_ShouldPreventOrphanedBookings()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();

        var invalidEventId = Guid.NewGuid();
        var booking = Models.Booking.CreatePending(invalidEventId);
        context.Bookings.Add(booking);

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    /// <summary>
    /// Проверяет наличие NOT NULL ограничений на обязательные колонки.
    /// </summary>
    [Fact]
    public async Task Migrations_ShouldEnforceNotNullConstraints()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();

        var connection = context.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT is_nullable
            FROM information_schema.columns
            WHERE table_schema = 'public'
            AND table_name = 'Events'
            AND column_name = 'Title';";
        await connection.OpenAsync();
        var isNullable = await command.ExecuteScalarAsync() as string;
        await connection.CloseAsync();

        Assert.Equal("NO", isNullable);
    }

    #region Вспомогательные методы

    private static async Task<bool> TableExistsAsync(AppDbContext context, string tableName)
    {
        var connection = context.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT FROM information_schema.tables 
                WHERE table_schema = 'public' 
                AND table_name = @tableName
            );";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "tableName";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        await connection.CloseAsync();

        return result is bool exists && exists;
    }

    private static async Task<bool> ForeignKeyExistsAsync(
        AppDbContext context,
        string fromTable,
        string fromColumn,
        string toTable,
        string toColumn)
    {
        var connection = context.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT FROM information_schema.table_constraints tc
                JOIN information_schema.key_column_usage kcu
                    ON tc.constraint_name = kcu.constraint_name
                JOIN information_schema.constraint_column_usage ccu
                    ON ccu.constraint_name = tc.constraint_name
                WHERE tc.constraint_type = 'FOREIGN KEY'
                AND tc.table_schema = 'public'
                AND tc.table_name = @fromTable
                AND kcu.column_name = @fromColumn
                AND ccu.table_name = @toTable
                AND ccu.column_name = @toColumn
            );";
        var parameters = new[]
        {
            ("fromTable", fromTable),
            ("fromColumn", fromColumn),
            ("toTable", toTable),
            ("toColumn", toColumn)
        };
        foreach (var (name, value) in parameters)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            command.Parameters.Add(param);
        }

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        await connection.CloseAsync();

        return result is bool exists && exists;
    }

    #endregion
}