using FlightInformation.API.Controllers;
using Infrastructure.Persistence;
using MediatR;
using FlightInfo = Infrastructure.Persistence.Entities.FlightInfo;

namespace Application.FlightInformation.Commands
{
    public record CreateFlightCommandRequest(FlightDetail FlightDetail) : IRequest<CreateFlightCommandResponse>;
    public class CreateFlightCommandResponse
    {
        public int? CreatedFlightId { get; set; }
    }

    public class CreateFlightCommandHandler(IApplicationDbContext dbContext)
        : IRequestHandler<CreateFlightCommandRequest, CreateFlightCommandResponse>
    {
        private readonly IApplicationDbContext _dbContext = dbContext;

        public async Task<CreateFlightCommandResponse> Handle(CreateFlightCommandRequest request, CancellationToken cancellationToken)
        {
            var flightDetail = request.FlightDetail;

            var flightInfo = new FlightInfo
            {
                Airline = flightDetail.Airline,
                FlightNumber = flightDetail.FlightNumber,
                DepartureAirport = flightDetail.DepartureAirport,
                ArrivalAirport = flightDetail.ArrivalAirport,
                DepartureTime = flightDetail.DepartureTime.DateTime,
                ArrivalTime = flightDetail.ArrivalTime.DateTime,
                Status = flightDetail.Status.ToString()
            };

            _dbContext.FlightInfo.Add(flightInfo);

            var rows = await _dbContext.SaveChangesAsync(cancellationToken);

            if (rows == 0)
            {
                return new CreateFlightCommandResponse { CreatedFlightId = null };
            }

            return new CreateFlightCommandResponse { CreatedFlightId = flightInfo.Id };

        }
    }
}
