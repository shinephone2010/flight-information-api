using Application.FlightInformation.Queries;
using FlightInformation.API.Controllers;
using FluentAssertions;
using Infrastructure.Persistence;

namespace FlightInformationAPI.UnitTests
{
    public class SearchFlightQueryHandlerTests : IClassFixture<FlightInformationAPITestsFixture>
    {
        private readonly FlightInformationAPITestsFixture _fixture;

        public SearchFlightQueryHandlerTests(FlightInformationAPITestsFixture fixture)
        {
            _fixture = fixture;
        }

        private static SearchFlightQueryHandler Sut(IApplicationDbContext dbContext)
            => new SearchFlightQueryHandler(dbContext);

        [Fact]
        public async Task Handle_NoFilters_ReturnsAllFlights()
        {
            // Arrange
            var dbContext = _fixture.Database;
            var handler = Sut(dbContext);

            var searchKeys = new SearchKeys
            {
                Airline = null,
                DepartureAirport = null,
                ArrivalAirport = null,
                FromDate = default,
                ToDate = default
            };

            var request = new SearchFlightsQueryRequest(searchKeys);
            var expectedCount = dbContext.FlightInfo.Count();

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Flights.Should().NotBeNull();
            response.Flights!.Count.Should().Be(expectedCount);
        }

        [Fact]
        public async Task Handle_FilterByAirline_IsCaseInsensitive_AndReturnsOnlyMatchingFlights()
        {
            // Arrange
            var dbContext = _fixture.Database;
            var handler = Sut(dbContext);

            var searchKeys = new SearchKeys
            {
                Airline = "air new zealand",
                DepartureAirport = null,
                ArrivalAirport = null,
                FromDate = default,
                ToDate = default
            };

            var request = new SearchFlightsQueryRequest(searchKeys);

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Flights.Should().NotBeNull();
            response.Flights!.Should().HaveCount(1);

            var flight = response.Flights.Single();
            flight.Airline.Should().Be("Air New Zealand");
            flight.FlightNumber.Should().Be("AI101");
        }

        [Fact]
        public async Task Handle_FilterByDepartureAndArrivalAndDateRange_ReturnsExpectedFlight()
        {
            // Arrange
            var dbContext = _fixture.Database;
            var handler = Sut(dbContext);

            var fromDate = new DateTime(2024, 11, 20);
            var toDate = new DateTime(2024, 11, 22);

            var searchKeys = new SearchKeys
            {
                Airline = "Virgin Australia",
                DepartureAirport = "AKL",
                ArrivalAirport = "DXB",
                FromDate = fromDate,
                ToDate = toDate
            };

            var request = new SearchFlightsQueryRequest(searchKeys);

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Flights.Should().NotBeNull();
            response.Flights!.Should().HaveCount(1);

            var flight = response.Flights.Single();
            flight.FlightNumber.Should().Be("VI102");
            flight.Airline.Should().Be("Virgin Australia");
            flight.DepartureAirport.Should().Be("AKL");
            flight.ArrivalAirport.Should().Be("DXB");
            flight.DepartureTime.Date.Should().Be(fromDate.Date);
            flight.ArrivalTime.Date.Should().Be(new DateTime(2024, 11, 21).Date);
        }

        [Fact]
        public async Task Handle_FilterByDateRangeWithNoMatches_ReturnsEmptyList()
        {
            // Arrange
            var dbContext = _fixture.Database;
            var handler = Sut(dbContext);

            var searchKeys = new SearchKeys
            {
                Airline = null,
                DepartureAirport = null,
                ArrivalAirport = null,
                FromDate = new DateTime(2025, 01, 01),
                ToDate = new DateTime(2025, 01, 02)
            };

            var request = new SearchFlightsQueryRequest(searchKeys);

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Flights.Should().NotBeNull();
            response.Flights.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_FilterByNonExistingAirline_ReturnsEmptyList()
        {
            // Arrange
            var dbContext = _fixture.Database;
            var handler = Sut(dbContext);

            var searchKeys = new SearchKeys
            {
                Airline = "NonExisting Airline",
                DepartureAirport = null,
                ArrivalAirport = null,
                FromDate = default,
                ToDate = default
            };

            var request = new SearchFlightsQueryRequest(searchKeys);

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Flights.Should().NotBeNull();
            response.Flights.Should().BeEmpty();
        }
    }
}
