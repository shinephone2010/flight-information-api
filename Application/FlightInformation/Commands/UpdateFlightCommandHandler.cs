using FlightInformation.API.Controllers;
using Infrastructure.Persistence;
using MediatR;

namespace Application.FlightInformation.Commands
{
    public record UpdateFlightCommandRequest(int Id, Flight Flight)
        : IRequest<UpdateFlightCommandResponse>;

    public class UpdateFlightCommandResponse()
    {
        public Flight? Flight { get; set; }
    }

    public class UpdateFlightCommandHandler(IApplicationDbContext dbContext)
        : IRequestHandler<UpdateFlightCommandRequest, UpdateFlightCommandResponse>
    {
        private readonly IApplicationDbContext _dbContext = dbContext;

        public async Task<UpdateFlightCommandResponse> Handle(UpdateFlightCommandRequest request, CancellationToken cancellationToken)
        {
            var flightInfo = await _dbContext.FlightInfo
                .FindAsync([request.Id], cancellationToken);

            if (flightInfo == null)
            {
                return new UpdateFlightCommandResponse
                {
                    Flight = null
                };
            }

            var flight = request.Flight;

            flightInfo.Airline = flight.Airline;
            flightInfo.FlightNumber = flight.FlightNumber;
            flightInfo.DepartureAirport = flight.DepartureAirport;
            flightInfo.ArrivalAirport = flight.ArrivalAirport;
            flightInfo.DepartureTime = flight.DepartureTime.DateTime;
            flightInfo.ArrivalTime = flight.ArrivalTime.DateTime;
            flightInfo.Status = flight.Status.ToString();

            // This will check in the database if the flight id and the last modified are both matched with the searched flight,
            // if someone already update the same flight, the lat modified will be updated, and the SaveChangesAsync will return 0
            // indicate no flight has been updated, at this point the database will throw the DbUpdateConcurrencyException
            _dbContext.Entry(flightInfo)
                .Property(f => f.LastModified)
                .OriginalValue = flightInfo.LastModified.AddHours(1);

            await _dbContext.SaveChangesAsync(cancellationToken);

            flightInfo.LastModified = flight.LastModified;

            return new UpdateFlightCommandResponse
            {
                Flight = flight
            };
        }
    }
}
