using Application.FlightInformation.Queries;
using FluentAssertions;
using Infrastructure.Persistence;

namespace FlightInformationAPI.UnitTests
{
    public class GetAllFlightsQueryHandlerTests : IClassFixture<FlightInformationAPITestsFixture>
    {
        private readonly FlightInformationAPITestsFixture _fixture;

        public GetAllFlightsQueryHandlerTests(FlightInformationAPITestsFixture fixture)
        {
            _fixture = fixture;
        }

        private static GetAllFlightsQueryHandler Sut(IApplicationDbContext dbContext)
            => new GetAllFlightsQueryHandler(dbContext);

        [Fact]
        public async Task Handle_WhenFlightsExist_ReturnsAllFlightsMapped()
        {
            // Arrange
            var dbContext = _fixture.Database;
            var handler = Sut(dbContext);
            var request = new GetAllFlightsQueryRequest();

            var expectedCount = dbContext.FlightInfo.Count();

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.AllFlights.Should().NotBeNull();
            response.AllFlights!.Count.Should().Be(expectedCount);

            var seededEntity = dbContext.FlightInfo.Single(f => f.Id == 1);

            response.AllFlights.Should().ContainSingle(f =>
                f.FlightNumber == seededEntity.FlightNumber &&
                f.Airline == seededEntity.Airline &&
                f.DepartureAirport == seededEntity.DepartureAirport &&
                f.ArrivalAirport == seededEntity.ArrivalAirport
            );
        }

        [Fact]
        public async Task Handle_WhenNoFlightsExist_ReturnsEmptyList()
        {
            // Remove all the entries
            var allFlights = _fixture.Database.FlightInfo.ToList();
            _fixture.Database.FlightInfo.RemoveRange(allFlights);
            _fixture.Database.SaveChanges();

            // Arrange
            var handler = Sut(_fixture.Database);
            var request = new GetAllFlightsQueryRequest();

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.AllFlights.Should().NotBeNull();
            response.AllFlights.Should().BeEmpty();
        }
    }
}
