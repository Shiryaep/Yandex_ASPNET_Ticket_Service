using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using Yandex_ASPNET_Ticket_Service.DataAccess;
using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Repositories;

namespace Yandex_ASPNET_Ticket_Service.IntegrationTests;

public class BookingRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public BookingRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task ResetDatabaseAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }

    private AppDbContext CreateContext()
    {
        return _fixture.CreateContext();
    }

    private async Task<Event> CreateTestEventAsync()
    {
        await using var context = CreateContext();
        var eventRepository = new EventRepository(context);

        var eventEntity = Event.Create(
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            100);

        await eventRepository.AddEventAsync(eventEntity);
        await eventRepository.SaveChangesAsync();
        return eventEntity;
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenBookingExists_ReturnsBooking()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var bookingRepository = new BookingRepository(context);
        var testEvent = await CreateTestEventAsync();
        var booking = Booking.CreatePending(testEvent.Id);
        await bookingRepository.AddBookingAsync(booking);
        await bookingRepository.SaveChangesAsync();

        // Act
        await using var verifyContext = CreateContext();
        var bookingRepositoryVerify = new BookingRepository(verifyContext);
        var result = await bookingRepositoryVerify.GetBookingByIdAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(testEvent.Id, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenBookingDoesNotExist_ReturnsNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var bookingRepository = new BookingRepository(context);

        // Act
        var result = await bookingRepository.GetBookingByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddBookingAsync_AddsBookingToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var bookingRepository = new BookingRepository(context);
        var testEvent = await CreateTestEventAsync();
        var booking = Booking.CreatePending(testEvent.Id);

        // Act
        await bookingRepository.AddBookingAsync(booking);
        await bookingRepository.SaveChangesAsync();

        // Assert - use a separate context to verify
        await using var verificationContext = CreateContext();
        var savedBooking = await verificationContext.Bookings
            .Include(b => b.Event)
            .FirstOrDefaultAsync(b => b.Id == booking.Id);

        Assert.NotNull(savedBooking);
        Assert.Equal(testEvent.Id, savedBooking.EventId);
        Assert.Equal(BookingStatus.Pending, savedBooking.Status);
    }

    [Fact]
    public async Task GetListOfPendingBookings_ReturnsOnlyPendingBookings()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var bookingRepository = new BookingRepository(context);
        var testEvent = await CreateTestEventAsync();

        var pendingBooking1 = Booking.CreatePending(testEvent.Id);
        var pendingBooking2 = Booking.CreatePending(testEvent.Id);
        var confirmedBooking = Booking.CreatePending(testEvent.Id);
        confirmedBooking.Confirm();

        await bookingRepository.AddBookingAsync(pendingBooking1);
        await bookingRepository.AddBookingAsync(pendingBooking2);
        await bookingRepository.AddBookingAsync(confirmedBooking);
        await bookingRepository.SaveChangesAsync();

        // Act
        var pendingIds = await bookingRepository.GetListOfPendingBookings();

        // Assert
        Assert.Equal(2, pendingIds.Count);
        Assert.Contains(pendingBooking1.Id, pendingIds);
        Assert.Contains(pendingBooking2.Id, pendingIds);
        Assert.DoesNotContain(confirmedBooking.Id, pendingIds);
    }

    [Fact]
    public async Task GetListOfPendingBookings_WhenNoPendingBookings_ReturnsEmptyList()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var bookingRepository = new BookingRepository(context);

        // Act
        var pendingIds = await bookingRepository.GetListOfPendingBookings();

        // Assert
        Assert.Empty(pendingIds);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsBookingChanges()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var bookingRepository = new BookingRepository(context);
        var testEvent = await CreateTestEventAsync();
        var booking = Booking.CreatePending(testEvent.Id);
        await bookingRepository.AddBookingAsync(booking);

        // Act
        await bookingRepository.SaveChangesAsync();

        // Assert - use a separate context to verify
        await using var verificationContext = CreateContext();
        var saved = await verificationContext.Bookings.FindAsync(booking.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Booking_CanBeConfirmedAndRejected()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var bookingRepository = new BookingRepository(context);
        var testEvent = await CreateTestEventAsync();
        var booking = Booking.CreatePending(testEvent.Id);
        await bookingRepository.AddBookingAsync(booking);
        await bookingRepository.SaveChangesAsync();

        // Act - Confirm
        booking.Confirm();
        await bookingRepository.SaveChangesAsync();

        // Assert
        var confirmedBooking = await bookingRepository.GetBookingByIdAsync(booking.Id);
        Assert.NotNull(confirmedBooking);
        Assert.Equal(BookingStatus.Confirmed, confirmedBooking.Status);
        Assert.NotNull(confirmedBooking.ProcessedAt);

        // Act - Reject (after reload)
        var bookingToReject = await bookingRepository.GetBookingByIdAsync(booking.Id);
        bookingToReject!.Reject();
        await bookingRepository.SaveChangesAsync();

        // Assert
        var rejectedBooking = await bookingRepository.GetBookingByIdAsync(booking.Id);
        Assert.Equal(BookingStatus.Rejected, rejectedBooking!.Status);
    }

    [Fact]
    public async Task Booking_EventRelationshipIsMaintained()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var bookingRepository = new BookingRepository(context);
        var testEvent = await CreateTestEventAsync();
        var booking = Booking.CreatePending(testEvent.Id);
        await bookingRepository.AddBookingAsync(booking);
        await bookingRepository.SaveChangesAsync();

        // Act - use a separate context to verify
        await using var verificationContext = CreateContext();
        var savedBooking = await verificationContext.Bookings
            .Include(b => b.Event)
            .FirstOrDefaultAsync(b => b.Id == booking.Id);

        // Assert
        Assert.NotNull(savedBooking);
        Assert.NotNull(savedBooking.Event);
        Assert.Equal(testEvent.Id, savedBooking.Event.Id);
        Assert.Equal("Test Event", savedBooking.Event.Title);
    }
}