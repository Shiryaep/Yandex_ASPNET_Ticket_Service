using Microsoft.AspNetCore.Mvc;
using Moq;
using Yandex_ASPNET_Ticket_Service.Controllers;
using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Services.BookingServices;

namespace EventService_Tests;

public class BookingsControllerTests
{
    [Fact]
    public async Task GetBooking_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var mockBookingService = new Mock<IBookingService>();
        var controller = new BookingsController(mockBookingService.Object);
        var nonExistentBookingId = Guid.NewGuid();

        mockBookingService.Setup(s => s.GetBookingByIdAsync(nonExistentBookingId))
            .ReturnsAsync((Booking?)null);

        // Act
        var result = await controller.GetBooking(nonExistentBookingId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
        var response = notFoundResult.Value as dynamic;
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetBooking_ExistingId_ReturnsOkWithCorrectData()
    {
        // Arrange
        var mockBookingService = new Mock<IBookingService>();
        var controller = new BookingsController(mockBookingService.Object);
        var bookingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddMinutes(-5);
        var booking = new Booking(eventId)
        {
            Id = bookingId,
            Status = BookingStatus.Pending,
            CreatedAt = createdAt,
            ProcessedAt = null
        };

        mockBookingService.Setup(s => s.GetBookingByIdAsync(bookingId))
            .ReturnsAsync(booking);

        // Act
        var result = await controller.GetBooking(bookingId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        var dto = okResult.Value as Yandex_ASPNET_Ticket_Service.Models.DTO.BookingResponseDto;
        Assert.NotNull(dto);
        Assert.Equal(bookingId, dto.Id);
        Assert.Equal(eventId, dto.EventId);
        Assert.Equal(BookingStatus.Pending, dto.Status);
        Assert.Equal(createdAt, dto.CreatedAt);
        Assert.Null(dto.ProcessedAt);
    }
}