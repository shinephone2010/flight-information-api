using Application.FlightInformation.Commands;
using FlightInformation.API.Controllers;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FlightInformationAPI.UnitTests
{
    public class CreateFlightCommandHandlerTest(FlightInformationAPITestsFixture fixture) : IClassFixture<FlightInformationAPITestsFixture>
    {
        private readonly FlightInformationAPITestsFixture _fixture = fixture;

        private static CreateFlightCommandHandler Sut(IApplicationDbContext dbContext)
            => new CreateFlightCommandHandler(dbContext);

        [Fact]
        public async Task Handle_ValidRequest_PersistsFlight_AndReturnsCreatedId()
        {
            // Arrange
            var initialCount = _fixture.Database.FlightInfo.Count();

            var flightDetail = new FlightDetail
            {
                Airline = "Test Airline",
                FlightNumber = "TA100",
                DepartureAirport = "WLG",
                ArrivalAirport = "AKL",
                DepartureTime = new DateTimeOffset(2024, 11, 20, 10, 0, 0, TimeSpan.Zero),
                ArrivalTime = new DateTimeOffset(2024, 11, 20, 11, 0, 0, TimeSpan.Zero),
                Status = FlightDetailStatus.Delayed
            };

            var request = new CreateFlightCommandRequest(flightDetail);

            // Act
            var response = await Sut(_fixture.Database).Handle(request, CancellationToken.None);

            // Assert: response
            response.Should().NotBeNull();
            response.CreatedFlightId.Should().NotBeNull();
            response.CreatedFlightId.Should().BeGreaterThan(0);

            // Assert: a new row was added
            var newCount = _fixture.Database.FlightInfo.Count();
            newCount.Should().Be(initialCount + 1);

            // Assert: data was mapped correctly
            var createdId = response.CreatedFlightId!.Value;
            var createdEntity = _fixture.Database.FlightInfo.SingleOrDefault(f => f.Id == createdId);

            createdEntity.Should().NotBeNull();
            createdEntity!.Airline.Should().Be(flightDetail.Airline);
            createdEntity.FlightNumber.Should().Be(flightDetail.FlightNumber);
            createdEntity.DepartureAirport.Should().Be(flightDetail.DepartureAirport);
            createdEntity.ArrivalAirport.Should().Be(flightDetail.ArrivalAirport);
            createdEntity.DepartureTime.Should().Be(flightDetail.DepartureTime.DateTime);
            createdEntity.ArrivalTime.Should().Be(flightDetail.ArrivalTime.DateTime);
            createdEntity.Status.Should().Be(flightDetail.Status.ToString());
        }

        [Fact]
        public async Task Handle_SaveChangesReturnsZero_ReturnsNullCreatedFlightId()
        {
            // Arrange
            var flightDetail = new FlightDetail
            {
                Airline = "Test Airline",
                FlightNumber = "TA200",
                DepartureAirport = "WLG",
                ArrivalAirport = "AKL",
                DepartureTime = new DateTimeOffset(2024, 11, 21, 10, 0, 0, TimeSpan.Zero),
                ArrivalTime = new DateTimeOffset(2024, 11, 21, 11, 0, 0, TimeSpan.Zero),
                Status = FlightDetailStatus.Delayed
            };

            var request = new CreateFlightCommandRequest(flightDetail);

            // Mock IApplicationDbContext to force SaveChangesAsync to return 0
            var dbContextMock = new Mock<IApplicationDbContext>();

            var flightInfoDbSetMock = new Mock<DbSet<FlightInfo>>();
            dbContextMock.Setup(c => c.FlightInfo).Returns(flightInfoDbSetMock.Object);

            dbContextMock
                .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act
            var response = await Sut(dbContextMock.Object).Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.CreatedFlightId.Should().BeNull();

            // Ensure we tried to add an entity and save changes
            flightInfoDbSetMock.Verify(s => s.Add(It.Is<FlightInfo>(
                f => f.Airline.Equals(flightDetail.Airline)
                && f.FlightNumber.Equals(flightDetail.FlightNumber)
                && f.DepartureAirport.Equals(flightDetail.DepartureAirport)
                && f.ArrivalAirport.Equals(flightDetail.ArrivalAirport)
                && f.DepartureTime.Equals(flightDetail.DepartureTime.DateTime)
                && f.ArrivalTime.Equals(flightDetail.ArrivalTime.DateTime)
                && f.Status.Equals(flightDetail.Status.ToString()))), Times.Once);

            dbContextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}
