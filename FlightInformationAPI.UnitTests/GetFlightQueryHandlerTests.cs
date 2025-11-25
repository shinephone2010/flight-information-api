using Application.FlightInformation.Queries;
using FluentAssertions;
using Infrastructure.Persistence;

namespace FlightInformationAPI.UnitTests
{
    public class GetFlightQueryHandlerTests : IClassFixture<FlightInformationAPITestsFixture>
    {
        private readonly FlightInformationAPITestsFixture _fixture;

        public GetFlightQueryHandlerTests(FlightInformationAPITestsFixture fixture)
        {
            _fixture = fixture;
        }

        private static GetFlightQueryHandler Sut(IApplicationDbContext dbContext)
            => new GetFlightQueryHandler(dbContext);

        [Fact]
        public async Task Handle_FlightExists_ReturnsMappedFlightDetail()
        {
            // Arrange
            var dbContext = _fixture.Database;
            var handler = Sut(dbContext);

            var seededEntity = dbContext.FlightInfo.Single(f => f.Id == 1);

            var request = new GetFlightQueryRequest(seededEntity.Id);

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.FlightDetail.Should().NotBeNull();

            var detail = response.FlightDetail!;

            detail.FlightNumber.Should().Be(seededEntity.FlightNumber);
            detail.Airline.Should().Be(seededEntity.Airline);
            detail.DepartureAirport.Should().Be(seededEntity.DepartureAirport);
            detail.ArrivalAirport.Should().Be(seededEntity.ArrivalAirport);
            detail.DepartureTime.DateTime.Should().Be(seededEntity.DepartureTime);
            detail.ArrivalTime.DateTime.Should().Be(seededEntity.ArrivalTime);
            detail.Status.ToString().Should().Be(seededEntity.Status);
        }

        [Fact]
        public async Task Handle_FlightDoesNotExist_ReturnsNullFlightDetail()
        {
            // Arrange
            var dbContext = _fixture.Database;
            var handler = Sut(dbContext);

            var nonExistingId = 9999;
            var request = new GetFlightQueryRequest(nonExistingId);

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.FlightDetail.Should().BeNull();
        }
    }
}
