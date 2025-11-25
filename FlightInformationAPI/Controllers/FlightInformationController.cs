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
    public class FlightInformationController(IMediator mediator, IValidator<FlightDetail> validator) : FlightInformationControllerControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly IValidator<FlightDetail> _validator = validator;

        public override async Task<IActionResult> CreateFlight([FromBody] FlightDetail body, CancellationToken cancellationToken = default)
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

            if (response.CreatedFlightId is null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            // 201 + Location header
            return CreatedAtAction(
                nameof(GetFlight),                
                new { id = response.CreatedFlightId },
                "Flight created successfully."
            );
        }

        public override async Task<ActionResult<FlightDetail>> DeleteFlight([BindRequired] int id, CancellationToken cancellationToken = default)
        {
            var deleteFlightCommandRequest = new DeleteFlightCommandRequest(id);

            var response = await _mediator.Send(deleteFlightCommandRequest, cancellationToken);

            if (response.FlightDetail == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "Not Found",
                    Message = $"Flight with id: {id} is not found."
                });
            }

            return Ok(response.FlightDetail);
        }

        public override async Task<ActionResult<FlightDetail>> GetFlight([BindRequired] int id, CancellationToken cancellationToken = default)
        {
            var getFlightQueryRequest = new GetFlightQueryRequest(id);

            var response = await _mediator.Send(getFlightQueryRequest, cancellationToken);

            if (response.FlightDetail is null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "Not Found",
                    Message = $"Flight with id: {id} is not found."
                });
            }

            return Ok(response.FlightDetail);
        }

        // No pagination return all the flights just for simplicity 
        public override async Task<ActionResult<ICollection<FlightDetail>>> GetAllFlights(CancellationToken cancellationToken = default)
        {
            var getAllFlightsQueryRequest = new GetAllFlightsQueryRequest();

            var response = await _mediator.Send(getAllFlightsQueryRequest, cancellationToken);

            return Ok(response.AllFlights);
        }

        public override async Task<ActionResult<ICollection<FlightDetail>>> SearchFlights(
            [BindRequired, FromQuery] SearchKeys searchKeys,
            CancellationToken cancellationToken = default)
        {
            var searchFlightsQueryRequest = new SearchFlightsQueryRequest(searchKeys);

            var response = await _mediator.Send(searchFlightsQueryRequest, cancellationToken);

            return Ok(response.Flights);
        }

        public override async Task<ActionResult<FlightDetail>> UpdateFlight(
            [BindRequired] int id,
            [FromBody] FlightDetail body,
            CancellationToken cancellationToken = default)
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
                var response = await _mediator.Send(commandRequest, cancellationToken);

                if (response.FlightDetail is null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Error = "Not Found",
                        Message = $"Flight with id: {id} is not found."
                    });
                }

                return Ok(response.FlightDetail);
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
