using Application.FlightInformation.Commands;
using FlightInformation.API.Controllers;
using FluentAssertions;
using Infrastructure.Persistence;

namespace FlightInformationAPI.UnitTests
{
    public class DeleteFlightCommandHandlerTests(FlightInformationAPITestsFixture fixture)
        : IClassFixture<FlightInformationAPITestsFixture>
    {
        private readonly FlightInformationAPITestsFixture _fixture = fixture;

        private static DeleteFlightCommandHandler Sut(IApplicationDbContext dbContext)
            => new DeleteFlightCommandHandler(dbContext);

        [Fact]
        public async Task Handle_FlightExists_RemovesFlight_AndReturnsMappedFlightDetail()
        {
            // Arrange
            var dbContext = _fixture.Database;
            var handler = Sut(dbContext);

            // pick an existing id from the seeded data
            var existingId = 1;
            var initialCount = dbContext.FlightInfo.Count();

            // Act
            var response = await handler.Handle(
                new DeleteFlightCommandRequest(existingId),
                CancellationToken.None);

            // Assert: response
            response.Should().NotBeNull();
            response.FlightDetail.Should().NotBeNull();

            response.FlightDetail!.FlightNumber.Should().Be("AI101");
            response.FlightDetail.Airline.Should().Be("Air New Zealand");
            response.FlightDetail.DepartureAirport.Should().Be("CHC");
            response.FlightDetail.ArrivalAirport.Should().Be("SYD");
            response.FlightDetail.Status.Should().Be(FlightDetailStatus.Cancelled);

            // Assert: entity removed
            var newCount = dbContext.FlightInfo.Count();
            newCount.Should().Be(initialCount - 1);

            var deletedEntity = await dbContext.FlightInfo.FindAsync(existingId);
            deletedEntity.Should().BeNull();
        }

        [Fact]
        public async Task Handle_FlightDoesNotExist_ReturnsNullFlightDetail_AndDoesNotChangeDatabase()
        {
            // Arrange
            var dbContext = _fixture.Database;
            var handler = Sut(dbContext);

            var nonExistingId = 9999;
            var initialCount = dbContext.FlightInfo.Count();

            // Act
            var response = await handler.Handle(
                new DeleteFlightCommandRequest(nonExistingId),
                CancellationToken.None);

            // Assert: response
            response.Should().NotBeNull();
            response.FlightDetail.Should().BeNull();

            // Assert: no rows removed
            var newCount = dbContext.FlightInfo.Count();
            newCount.Should().Be(initialCount);
        }
    }
}
