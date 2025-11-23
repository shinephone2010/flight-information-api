using FlightInformation.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FlightInformationAPI.Controllers
{
    [ApiController]
    public class FlightInformationController : FlightInformationControllerControllerBase
    {
        public override Task<ActionResult<Flight>> CreateFlight(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<IActionResult> DeleteFlight([BindRequired] string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<ActionResult<Flight>> GetFlight([BindRequired] string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<ActionResult<ICollection<Flight>>> GetFlights(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<ActionResult<ICollection<Flight>>> SearchFlight([BindRequired, FromQuery] SearchKeys searchKeys, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<ActionResult<Flight>> UpdateFlight([BindRequired] string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
