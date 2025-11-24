using Application.FlightInformation.Commands;
using FlightInformation.API.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace FlightInformationAPI.Controllers
{
    [ApiController]
    public class FlightInformationController(IMediator mediator) : FlightInformationControllerControllerBase
    {
        private readonly IMediator _mediator = mediator;

        public override Task<IActionResult> CreateFlight([FromBody] Flight body, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<ActionResult<Flight>> DeleteFlight([BindRequired] int id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<ActionResult<Flight>> GetFlight([BindRequired] int id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<ActionResult<ICollection<Flight>>> GetFlights(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<ActionResult<ICollection<Flight>>> SearchFlights([BindRequired, FromQuery] SearchKeys searchKeys, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override async Task<ActionResult<Flight>> UpdateFlight([BindRequired] int id, [FromBody] Flight body, CancellationToken cancellationToken = default)
        {
            var commandRequest = new UpdateFlightCommandRequest(id, body);

            try
            {
                var flight = await _mediator.Send(commandRequest, cancellationToken);

                if (flight.FlightInformation is null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Error = "Not Found",
                        Message = $"Flight with id: {id} is not found."
                    });
                }

                return Ok(flight);
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
