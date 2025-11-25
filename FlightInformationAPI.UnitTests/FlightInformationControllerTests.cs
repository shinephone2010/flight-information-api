using Application.FlightInformation.Commands;
using Application.FlightInformation.Queries;
using FlightInformation.API.Controllers;
using FlightInformationAPI.Controllers;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FlightInformationAPI.UnitTests
{
    public class FlightInformationControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IValidator<FlightDetail>> _validatorMock;
        private readonly Mock<ILogger<FlightInformationController>> _loggerMock;
        private readonly FlightInformationController _sut;

        public FlightInformationControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _validatorMock = new Mock<IValidator<FlightDetail>>();
            _loggerMock = new Mock<ILogger<FlightInformationController>>();

            _sut = new FlightInformationController(
                _mediatorMock.Object,
                _validatorMock.Object,
                _loggerMock.Object);
        }

        #region CreateFlight

        [Fact]
        public async Task CreateFlight_ValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var body = new FlightDetail
            {
                FlightNumber = "NZ123",
                Airline = "NZ",
                DepartureAirport = "WLG",
                ArrivalAirport = "AKL"
            };

            _validatorMock
                .Setup(v => v.Validate(body))
                .Returns(new ValidationResult()); // no errors => IsValid == true

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<CreateFlightCommandRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateFlightCommandResponse
                {
                    CreatedFlightId = 42
                });

            // Act
            var result = await _sut.CreateFlight(body, CancellationToken.None);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(FlightInformationController.GetFlight), created.ActionName);
            Assert.Equal(42, created!.RouteValues!["id"]);
            Assert.Equal("Flight created successfully.", created.Value);
        }

        [Fact]
        public async Task CreateFlight_InvalidModel_ReturnsBadRequest_AndSkipsMediator()
        {
            // Arrange
            var body = new FlightDetail
            {
                FlightNumber = "" // invalid
            };

            var validationResult = new ValidationResult(new[]
            {
                new ValidationFailure("FlightNumber", "Flight number is required.")
            });

            _validatorMock
                .Setup(v => v.Validate(body))
                .Returns(validationResult);

            // Act
            var result = await _sut.CreateFlight(body, CancellationToken.None);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequest.Value);

            Assert.Equal("Validation Failure", errorResponse.Error);
            Assert.Equal("Invalid data format", errorResponse.Message);
            Assert.Single(errorResponse.Details);
            Assert.Contains("FlightNumber", errorResponse.Details[0]);

            _mediatorMock.Verify(m =>
                    m.Send(It.IsAny<CreateFlightCommandRequest>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateFlight_MediatorReturnsNullId_Returns500()
        {
            // Arrange
            var body = new FlightDetail
            {
                FlightNumber = "NZ123"
            };

            _validatorMock
                .Setup(v => v.Validate(body))
                .Returns(new ValidationResult()); // valid

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<CreateFlightCommandRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateFlightCommandResponse
                {
                    CreatedFlightId = null
                });

            // Act
            var result = await _sut.CreateFlight(body, CancellationToken.None);

            // Assert
            var objResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objResult.StatusCode);
        }

        #endregion

        #region DeleteFlight

        [Fact]
        public async Task DeleteFlight_FlightExists_ReturnsOkWithFlight()
        {
            // Arrange
            var id = 10;

            var flight = new FlightDetail
            {
                FlightNumber = "NZ123"
            };

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<DeleteFlightCommandRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteFlightCommandResponse
                {
                    FlightDetail = flight
                });

            // Act
            var result = await _sut.DeleteFlight(id, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returnedFlight = Assert.IsType<FlightDetail>(ok.Value);
            Assert.Equal(flight.FlightNumber, returnedFlight.FlightNumber);
        }

        [Fact]
        public async Task DeleteFlight_FlightDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var id = 999;

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<DeleteFlightCommandRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteFlightCommandResponse
                {
                    FlightDetail = null
                });

            // Act
            var result = await _sut.DeleteFlight(id, CancellationToken.None);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            var errorResponse = Assert.IsType<ErrorResponse>(notFound.Value);
            Assert.Equal("Not Found", errorResponse.Error);
            Assert.Contains(id.ToString(), errorResponse.Message);
        }

        #endregion

        #region GetFlight

        [Fact]
        public async Task GetFlight_Found_ReturnsOkWithFlight()
        {
            // Arrange
            var id = 1;
            var flight = new FlightDetail { FlightNumber = "NZ123" };

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetFlightQueryRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFlightQueryResponse
                {
                    FlightDetail = flight
                });

            // Act
            var result = await _sut.GetFlight(id, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<FlightDetail>(ok.Value);
            Assert.Equal("NZ123", returned.FlightNumber);
        }

        [Fact]
        public async Task GetFlight_NotFound_ReturnsNotFound()
        {
            // Arrange
            var id = 999;

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetFlightQueryRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFlightQueryResponse
                {
                    FlightDetail = null
                });

            // Act
            var result = await _sut.GetFlight(id, CancellationToken.None);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            var errorResponse = Assert.IsType<ErrorResponse>(notFound.Value);
            Assert.Equal("Not Found", errorResponse.Error);
        }

        #endregion

        #region GetAllFlights

        [Fact]
        public async Task GetAllFlights_ReturnsOkWithFlights()
        {
            // Arrange
            var flights = new List<FlightDetail>
            {
                new FlightDetail { FlightNumber = "NZ123" },
                new FlightDetail { FlightNumber = "NZ456" }
            };

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetAllFlightsQueryRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAllFlightsQueryResponse
                {
                    AllFlights = flights
                });

            // Act
            var result = await _sut.GetAllFlights(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsAssignableFrom<ICollection<FlightDetail>>(ok.Value);
            Assert.Equal(2, returned.Count);
        }

        #endregion

        #region SearchFlights

        [Fact]
        public async Task SearchFlights_ReturnsOkWithFlights()
        {
            // Arrange
            var searchKeys = new SearchKeys
            {
                Airline = "NZ",
                DepartureAirport = "WLG",
                ArrivalAirport = "AKL"
            };

            var flights = new List<FlightDetail>
            {
                new FlightDetail { FlightNumber = "NZ123" }
            };

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<SearchFlightsQueryRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SearchFlightQueryResponse
                {
                    Flights = flights
                });

            // Act
            var result = await _sut.SearchFlights(searchKeys, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsAssignableFrom<ICollection<FlightDetail>>(ok.Value);
            Assert.Single(returned);
            Assert.Equal("NZ123", returned.First().FlightNumber);
        }

        #endregion

        #region UpdateFlight

        [Fact]
        public async Task UpdateFlight_ValidAndFound_ReturnsOkWithFlight()
        {
            // Arrange
            var id = 1;
            var body = new FlightDetail { FlightNumber = "NZ123" };

            _validatorMock
                .Setup(v => v.Validate(body))
                .Returns(new ValidationResult()); // valid

            var updatedFlight = new FlightDetail { FlightNumber = "NZ123-UPDATED" };

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<UpdateFlightCommandRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateFlightCommandResponse
                {
                    FlightDetail = updatedFlight
                });

            // Act
            var result = await _sut.UpdateFlight(id, body, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<FlightDetail>(ok.Value);
            Assert.Equal("NZ123-UPDATED", returned.FlightNumber);
        }

        [Fact]
        public async Task UpdateFlight_InvalidModel_ReturnsBadRequest_AndSkipsMediator()
        {
            // Arrange
            var id = 1;
            var body = new FlightDetail { FlightNumber = "" };

            var validationResult = new ValidationResult(new[]
            {
                new ValidationFailure("FlightNumber", "Flight number is required.")
            });

            _validatorMock
                .Setup(v => v.Validate(body))
                .Returns(validationResult);

            // Act
            var result = await _sut.UpdateFlight(id, body, CancellationToken.None);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequest.Value);
            Assert.Equal("Validation Failure", errorResponse.Error);

            _mediatorMock.Verify(m =>
                    m.Send(It.IsAny<UpdateFlightCommandRequest>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateFlight_NotFound_ReturnsNotFound()
        {
            // Arrange
            var id = 999;
            var body = new FlightDetail { FlightNumber = "NZ123" };

            _validatorMock
                .Setup(v => v.Validate(body))
                .Returns(new ValidationResult()); // valid

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<UpdateFlightCommandRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateFlightCommandResponse
                {
                    FlightDetail = null
                });

            // Act
            var result = await _sut.UpdateFlight(id, body, CancellationToken.None);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            var errorResponse = Assert.IsType<ErrorResponse>(notFound.Value);
            Assert.Equal("Not Found", errorResponse.Error);
            Assert.Contains(id.ToString(), errorResponse.Message);
        }

        [Fact]
        public async Task UpdateFlight_ConcurrencyException_ReturnsConflict()
        {
            // Arrange
            var id = 1;
            var body = new FlightDetail { FlightNumber = "NZ123" };

            _validatorMock
                .Setup(v => v.Validate(body))
                .Returns(new ValidationResult()); // valid

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<UpdateFlightCommandRequest>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            // Act
            var result = await _sut.UpdateFlight(id, body, CancellationToken.None);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
            var errorResponse = Assert.IsType<ErrorResponse>(conflict.Value);
            Assert.Equal("Concurrency Conflict", errorResponse.Error);
        }

        #endregion
    }
}
