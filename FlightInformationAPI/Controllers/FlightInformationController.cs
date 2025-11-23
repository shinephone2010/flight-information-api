using FlightInformation.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FlightInformationAPI.Controllers
{
    public class FlightInformationController : FlightInformationControllerControllerBase
    {
        [HttpPost]
        public override Task<ActionResult<Flight>> CreateFlight(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        public override Task<IActionResult> DeleteFlight([BindRequired] string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public override Task<ActionResult<Flight>> GetFlight([BindRequired] string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public override Task<ActionResult<ICollection<Flight>>> GetFlights(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public override Task<ActionResult<ICollection<Flight>>> SearchFlight([FromQuery] string airline = null, [FromQuery] string departureAirport = null, [FromQuery] string arrivalAirport = null, [FromQuery] string fromDate = null, [FromQuery] string toDate = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        public override Task<ActionResult<Flight>> UpdateFlight([BindRequired] string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
