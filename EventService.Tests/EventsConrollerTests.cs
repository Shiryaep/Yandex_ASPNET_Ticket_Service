using Microsoft.AspNetCore.Mvc;
using Moq;
using Yandex_ASPNET_Ticket_Service.Controllers;
using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Services.BookingServices;
using Yandex_ASPNET_Ticket_Service.Services.EventServices;

namespace EventService_Tests
{
    public class EventsConrollerTests
    {
        [Fact]
        public void Post_EndAtEarlierThanStartAt_ModelStateInvalid()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            var mockBookingService = new Mock<IBookingService>();
            var controller = new EventsController(mockEventService.Object, mockBookingService.Object);
            controller.ModelState.AddModelError("Error", "Error");

            // Act
            var result = controller.Post(null!);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            mockEventService.Verify(service => service.AddEvent(It.IsAny<Event>()), Times.Never);
        }

        [Fact]
        public void Put_EndAtEarlierThanStartAt_ModelStateInvalid()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            var mockBookingService = new Mock<IBookingService>();
            var controller = new EventsController(mockEventService.Object, mockBookingService.Object);
            controller.ModelState.AddModelError("Error", "Error");

            // Act
            var result = controller.Put(new Guid(), null!);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            mockEventService.Verify(service => service.UpdateEvent(It.IsAny<Guid>(), It.IsAny<Event>()), Times.Never);
        }

        [Fact]
        public async Task PostBook_NonExistentEvent_ReturnsNotFound()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            var mockBookingService = new Mock<IBookingService>();
            var controller = new EventsController(mockEventService.Object, mockBookingService.Object);
            var nonExistentEventId = Guid.NewGuid();

            mockEventService.Setup(s => s.GetEvent(nonExistentEventId)).Returns((Event?)null);

            // Act
            var result = await Task.Run(() => controller.Post(nonExistentEventId));

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            mockBookingService.Verify(s => s.CreateBookingAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task PostBook_DeletedEvent_ReturnsNotFound()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            var mockBookingService = new Mock<IBookingService>();
            var controller = new EventsController(mockEventService.Object, mockBookingService.Object);
            var deletedEventId = Guid.NewGuid();

            // Deleted event currently equal to non existent event
            mockEventService.Setup(s => s.GetEvent(deletedEventId)).Returns((Event?)null);

            // Act
            var result = await Task.Run(() => controller.Post(deletedEventId));

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            mockBookingService.Verify(s => s.CreateBookingAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}
