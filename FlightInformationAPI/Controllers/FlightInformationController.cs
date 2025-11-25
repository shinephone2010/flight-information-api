using Application.FlightInformation.Commands;
using Application.FlightInformation.Queries;
using FlightInformation.API.Controllers;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace FlightInformationAPI.Controllers
{
    [ApiController]
    public class FlightInformationController(
        IMediator mediator,
        IValidator<FlightDetail> validator,
        ILogger<FlightInformationController> logger) : FlightInformationControllerControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly IValidator<FlightDetail> _validator = validator;
        private readonly ILogger<FlightInformationController> _logger = logger;

        public override async Task<IActionResult> CreateFlight(
            [FromBody] FlightDetail body,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("CreateFlight called for FlightNumber {FlightNumber}", body.FlightNumber);

            ValidationResult result = _validator.Validate(body);

            if (!result.IsValid)
            {
                _logger.LogWarning(
                    "CreateFlight validation failed for FlightNumber {FlightNumber}. Errors: {@Errors}",
                    body.FlightNumber,
                    result.Errors);

                var errorResponse = new ErrorResponse()
                {
                    Error = "Validation Failure",
                    Message = "Invalid data format",
                    Details = []
                };

                errorResponse.Details
                    .AddRange([.. result.Errors.Select(err => $"{err.PropertyName}: {err.ErrorMessage}")]);

                return BadRequest(errorResponse);
            }

            var createFlightCommandRequest = new CreateFlightCommandRequest(body);
            var response = await _mediator.Send(createFlightCommandRequest, cancellationToken);

            if (response.CreatedFlightId is null)
            {
                _logger.LogError(
                    "CreateFlight failed. Mediator returned null CreatedFlightId for FlightNumber {FlightNumber}",
                    body.FlightNumber);

                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation(
                "Flight created successfully with Id {FlightId} for FlightNumber {FlightNumber}",
                response.CreatedFlightId,
                body.FlightNumber);

            // 201 + Location header
            return CreatedAtAction(
                nameof(GetFlight),
                new { id = response.CreatedFlightId },
                "Flight created successfully."
            );
        }

        public override async Task<ActionResult<FlightDetail>> DeleteFlight(
            [BindRequired] int id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("DeleteFlight called for Id {Id}", id);

            var deleteFlightCommandRequest = new DeleteFlightCommandRequest(id);
            var response = await _mediator.Send(deleteFlightCommandRequest, cancellationToken);

            if (response.FlightDetail == null)
            {
                _logger.LogWarning("DeleteFlight attempted for non-existing Id {Id}", id);

                return NotFound(new ErrorResponse
                {
                    Error = "Not Found",
                    Message = $"Flight with id: {id} is not found."
                });
            }

            _logger.LogInformation("DeleteFlight succeeded for Id {Id}", id);

            return Ok(response.FlightDetail);
        }

        public override async Task<ActionResult<FlightDetail>> GetFlight(
            [BindRequired] int id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("GetFlight called for Id {Id}", id);

            var getFlightQueryRequest = new GetFlightQueryRequest(id);
            var response = await _mediator.Send(getFlightQueryRequest, cancellationToken);

            if (response.FlightDetail is null)
            {
                _logger.LogWarning("GetFlight did not find a flight with Id {Id}", id);

                return NotFound(new ErrorResponse
                {
                    Error = "Not Found",
                    Message = $"Flight with id: {id} is not found."
                });
            }

            _logger.LogInformation("GetFlight succeeded for Id {Id}", id);

            return Ok(response.FlightDetail);
        }

        // No pagination return all the flights just for simplicity 
        public override async Task<ActionResult<ICollection<FlightDetail>>> GetAllFlights(
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("GetAllFlights called.");

            var getAllFlightsQueryRequest = new GetAllFlightsQueryRequest();
            var response = await _mediator.Send(getAllFlightsQueryRequest, cancellationToken);

            _logger.LogInformation("GetAllFlights returned {Count} flights.", response.AllFlights.Count);

            return Ok(response.AllFlights);
        }

        public override async Task<ActionResult<ICollection<FlightDetail>>> SearchFlights(
            [BindRequired, FromQuery] SearchKeys searchKeys,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "SearchFlights called with Airline {Airline}, Departure {DepartureAirport}, Arrival {ArrivalAirport}",
                searchKeys.Airline,
                searchKeys.DepartureAirport,
                searchKeys.ArrivalAirport);

            var searchFlightsQueryRequest = new SearchFlightsQueryRequest(searchKeys);
            var response = await _mediator.Send(searchFlightsQueryRequest, cancellationToken);

            _logger.LogInformation(
                "SearchFlights returned {Count} flights for Airline {Airline}",
                response.Flights.Count,
                searchKeys.Airline);

            return Ok(response.Flights);
        }

        public override async Task<ActionResult<FlightDetail>> UpdateFlight(
            [BindRequired] int id,
            [FromBody] FlightDetail body,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("UpdateFlight called for Id {Id}", id);

            ValidationResult result = _validator.Validate(body);

            if (!result.IsValid)
            {
                _logger.LogWarning(
                    "UpdateFlight validation failed for Id {Id}. Errors: {@Errors}",
                    id,
                    result.Errors);

                var errorResponse = new ErrorResponse()
                {
                    Error = "Validation Failure",
                    Message = "Invalid data format",
                    Details = []
                };

                errorResponse.Details
                    .AddRange([.. result.Errors.Select(err => $"{err.PropertyName}: {err.ErrorMessage}")]);

                return BadRequest(errorResponse);
            }

            var commandRequest = new UpdateFlightCommandRequest(id, body);

            try
            {
                var response = await _mediator.Send(commandRequest, cancellationToken);

                if (response.FlightDetail is null)
                {
                    _logger.LogWarning("UpdateFlight did not find a flight with Id {Id}", id);

                    return NotFound(new ErrorResponse
                    {
                        Error = "Not Found",
                        Message = $"Flight with id: {id} is not found."
                    });
                }

                _logger.LogInformation("UpdateFlight succeeded for Id {Id}", id);

                return Ok(response.FlightDetail);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Concurrency conflict when updating flight with Id {Id}",
                    id);

                return Conflict(new ErrorResponse
                {
                    Error = "Concurrency Conflict",
                    Message = "The flight was modified by another user. Please reload the latest data and try again."
                });
            }
        }
    }
}
