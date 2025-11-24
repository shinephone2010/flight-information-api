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
    public class FlightInformationController(IMediator mediator, IValidator<Flight> validator) : FlightInformationControllerControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly IValidator<Flight> _validator = validator;

        public override async Task<IActionResult> CreateFlight([FromBody] Flight body, CancellationToken cancellationToken = default)
        {
            ValidationResult result = _validator.Validate(body);

            if (!result.IsValid)
            {
                var errorResponse = new ErrorResponse()
                {
                    Error = "Validation Failure",
                    Message = "Invalid data format",
                    Details = []
                };

                foreach (var error in result.Errors)
                {
                    errorResponse.Details.Add($"{error.PropertyName}: {error.ErrorMessage}");
                }

                return BadRequest(errorResponse);
            }

            var createFlightCommandRequest = new CreateFlightCommandRequest(body);

            var response = await _mediator.Send(createFlightCommandRequest, cancellationToken);

            if (response < 1)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok("Flight created successfully.");
        }

        public override async Task<ActionResult<Flight>> DeleteFlight([BindRequired] int id, CancellationToken cancellationToken = default)
        {
            var deleteFlightCommandRequest = new DeleteFlightCommandRequest(id);

            var response = await _mediator.Send(deleteFlightCommandRequest, cancellationToken);

            if (response.Flight == null)
            {
                new ErrorResponse
                {
                    Error = "Not Found",
                    Message = $"Flight with id: {id} is not found."
                };
            }

            return Ok(response.Flight);
        }

        public override async Task<ActionResult<Flight>> GetFlight([BindRequired] int id, CancellationToken cancellationToken = default)
        {
            var getFlightQueryRequest = new GetFlightQueryRequest(id);

            var response = await _mediator.Send(getFlightQueryRequest, cancellationToken);

            if (response.Flight is null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "Not Found",
                    Message = $"Flight with id: {id} is not found."
                });
            }

            return Ok(response.Flight);
        }

        // No pagination just for simplicity 
        public override async Task<ActionResult<ICollection<Flight>>> GetFlights(CancellationToken cancellationToken = default)
        {
            var getAllFlightsQueryRequest = new GetAllFlightsQueryRequest();

            var response = await _mediator.Send(getAllFlightsQueryRequest, cancellationToken);

            return Ok(response.AllFlights);
        }

        public override async Task<ActionResult<ICollection<Flight>>> SearchFlights([BindRequired, FromQuery] SearchKeys searchKeys, CancellationToken cancellationToken = default)
        {
            var searchFlightsQueryRequest = new SearchFlightsQueryRequest(searchKeys);

            var response = await _mediator.Send(searchFlightsQueryRequest, cancellationToken);

            return Ok(response.Flights);
        }

        public override async Task<ActionResult<Flight>> UpdateFlight([BindRequired] int id, [FromBody] Flight body, CancellationToken cancellationToken = default)
        {
            ValidationResult result = _validator.Validate(body);

            if (!result.IsValid)
            {
                var errorResponse = new ErrorResponse()
                {
                    Error = "Validation Failure",
                    Message = "Invalid data format",
                    Details = []
                };

                foreach (var error in result.Errors)
                {
                    errorResponse.Details.Add($"{error.PropertyName}: {error.ErrorMessage}");
                }

                return BadRequest(errorResponse);
            }

            var commandRequest = new UpdateFlightCommandRequest(id, body);

            try
            {
                var flight = await _mediator.Send(commandRequest, cancellationToken);

                if (flight.Flight is null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Error = "Not Found",
                        Message = $"Flight with id: {id} is not found."
                    });
                }

                return Ok(flight.Flight);
            }
            catch (DbUpdateConcurrencyException)
            {

                return Conflict(new ErrorResponse
                {
                    Error = "Concurrency Conflict",
                    Message = "The flight was modified by another user. Please reload the latest data and try again."
                });
            }

        }
    }
}
