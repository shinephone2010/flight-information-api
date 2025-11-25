using Application.FlightInformation.Commands;
using FlightInformation.API.Controllers;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FlightInformationAPI.UnitTests
{
    public class UpdateFlightCommandHandlerTests : IClassFixture<FlightInformationAPITestsFixture>
    {
        private readonly FlightInformationAPITestsFixture _fixture;

        public UpdateFlightCommandHandlerTests(FlightInformationAPITestsFixture fixture)
        {
            _fixture = fixture;
        }

        private static UpdateFlightCommandHandler Sut(IApplicationDbContext dbContext)
            => new UpdateFlightCommandHandler(dbContext);

        [Fact]
        public async Task Handle_FlightExists_UpdatesEntityAndReturnsFlightDetail()
        {
            // Arrange
            var dbContext = _fixture.Database;
            var handler = Sut(dbContext);

            var existingId = 1;

            var updatedDetail = new FlightDetail
            {
                Airline = "Updated Airline",
                FlightNumber = "UPD123",
                DepartureAirport = "WLG",
                ArrivalAirport = "AKL",
                DepartureTime = new DateTimeOffset(2024, 11, 25, 8, 30, 0, TimeSpan.Zero),
                ArrivalTime = new DateTimeOffset(2024, 11, 25, 9, 45, 0, TimeSpan.Zero),
                Status = FlightDetailStatus.InAir
            };

            var request = new UpdateFlightCommandRequest(existingId, updatedDetail);

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert: response
            response.Should().NotBeNull();
            response.FlightDetail.Should().NotBeNull();
            response.FlightDetail.Should().BeEquivalentTo(updatedDetail);

            // Assert: entity actually updated in the database
            var updatedEntity = await dbContext.FlightInfo.FindAsync(existingId);
            updatedEntity.Should().NotBeNull();
            updatedEntity!.Airline.Should().Be(updatedDetail.Airline);
            updatedEntity.FlightNumber.Should().Be(updatedDetail.FlightNumber);
            updatedEntity.DepartureAirport.Should().Be(updatedDetail.DepartureAirport);
            updatedEntity.ArrivalAirport.Should().Be(updatedDetail.ArrivalAirport);
            updatedEntity.DepartureTime.Should().Be(updatedDetail.DepartureTime.DateTime);
            updatedEntity.ArrivalTime.Should().Be(updatedDetail.ArrivalTime.DateTime);
            updatedEntity.Status.Should().Be(updatedDetail.Status.ToString());
        }

        [Fact]
        public async Task Handle_FlightDoesNotExist_ReturnsNullFlightDetail_AndDoesNotChangeCount()
        {
            // Arrange
            var dbContext = _fixture.Database;
            var handler = Sut(dbContext);

            var nonExistingId = 9999;
            var initialCount = dbContext.FlightInfo.Count();

            var updateDetail = new FlightDetail
            {
                Airline = "Does Not Matter",
                FlightNumber = "NOPE999",
                DepartureAirport = "WLG",
                ArrivalAirport = "AKL",
                DepartureTime = new DateTimeOffset(2024, 11, 25, 8, 0, 0, TimeSpan.Zero),
                ArrivalTime = new DateTimeOffset(2024, 11, 25, 9, 0, 0, TimeSpan.Zero),
                Status = FlightDetailStatus.Scheduled
            };

            var request = new UpdateFlightCommandRequest(nonExistingId, updateDetail);

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert: response
            response.Should().NotBeNull();
            response.FlightDetail.Should().BeNull();

            // Assert: database unchanged
            var newCount = dbContext.FlightInfo.Count();
            newCount.Should().Be(initialCount);
        }

        [Fact]
        public async Task Handle_WhenSaveChangesThrowsConcurrencyException_PropagatesException()
        {
            // Arrange
            var flightDetail = new FlightDetail
            {
                Airline = "Updated Airline",
                FlightNumber = "UPD123",
                DepartureAirport = "WLG",
                ArrivalAirport = "AKL",
                DepartureTime = new DateTimeOffset(2024, 11, 25, 8, 30, 0, TimeSpan.Zero),
                ArrivalTime = new DateTimeOffset(2024, 11, 25, 9, 45, 0, TimeSpan.Zero),
                Status = FlightDetailStatus.InAir
            };

            var request = new UpdateFlightCommandRequest(1, flightDetail);

            var dbContextMock = new Mock<IApplicationDbContext>();

            var flightInfoSetMock = new Mock<DbSet<FlightInfo>>();

            var existingEntity = new FlightInfo
            {
                Id = 1,
                Airline = "Old Airline",
                FlightNumber = "OLD001",
                DepartureAirport = "OLD",
                ArrivalAirport = "OLD",
                DepartureTime = DateTime.UtcNow,
                ArrivalTime = DateTime.UtcNow.AddHours(1),
                Status = FlightDetailStatus.InAir.ToString(),
                LastModified = DateTimeOffset.UtcNow
            };

            dbContextMock
                .Setup(c => c.FlightInfo)
                .Returns(flightInfoSetMock.Object);

            dbContextMock
                .Setup(c => c.FlightInfo.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingEntity);


            dbContextMock.Setup(c => c.Entry(It.IsAny<FlightInfo>()))
                .Returns<FlightInfo>(f => _fixture.Database.Entry(f));

            dbContextMock
                .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateConcurrencyException("Simulated concurrency conflict"));

            var handler = new UpdateFlightCommandHandler(dbContextMock.Object);

            // Act
            var act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DbUpdateConcurrencyException>()
                .WithMessage("Simulated concurrency conflict*");
        }
    }
}
