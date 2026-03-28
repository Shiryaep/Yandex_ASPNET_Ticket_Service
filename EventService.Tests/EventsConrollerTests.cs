using Microsoft.AspNetCore.Mvc;
using Moq;
using Yandex_ASPNET_Ticket_Service.Controllers;
using Yandex_ASPNET_Ticket_Service.EventServices;
using Yandex_ASPNET_Ticket_Service.Models;

namespace EventService_Tests
{
    public class EventsConrollerTests
    {
        [Fact]
        public void Post_EndAtEarlierThanStartAt_ModelStateInvalid()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            var controller = new EventsController(mockEventService.Object);
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
            var controller = new EventsController(mockEventService.Object);
            controller.ModelState.AddModelError("Error", "Error");

            // Act
            var result = controller.Put(new Guid(), null!);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            mockEventService.Verify(service => service.UpdateEvent(It.IsAny<Guid>(), It.IsAny<Event>()), Times.Never);
        }
    }
}
