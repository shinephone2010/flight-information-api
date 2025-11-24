using FlightInformation.API.Controllers;
using Infrastructure.Persistence;
using MediatR;

namespace Application.FlightInformation.Commands
{
    public record UpdateFlightCommandRequest(int Id, Flight Flight)
        : IRequest<UpdateFlightCommandResponse>;

    public class UpdateFlightCommandResponse()
    {
        public Flight? FlightInformation { get; set; }
    }

    public class UpdateFlightCommandHandler(IApplicationDbContext dbContext)
        : IRequestHandler<UpdateFlightCommandRequest, UpdateFlightCommandResponse>
    {
        private readonly IApplicationDbContext _dbContext = dbContext;

        public async Task<UpdateFlightCommandResponse> Handle(UpdateFlightCommandRequest request, CancellationToken cancellationToken)
        {
            var flight = await _dbContext.FlightInformation
                .FindAsync([request.Id], cancellationToken);

            if (flight == null)
            {
                return new UpdateFlightCommandResponse
                {
                    FlightInformation = null
                };
            }

            var flightInformation = request.Flight;

            flight.Airline = flightInformation.Airline;
            flight.FlightNumber = flightInformation.FlightNumber;
            flight.ArrivalAirport = flightInformation.ArrivalAirport;
            flight.DepartureAirport = flightInformation.DepartureAirport;
            flight.ArrivalAirport = flightInformation.ArrivalAirport;
            flight.DepartureAirport = flightInformation.DepartureAirport;
            flight.Status = flightInformation.Status.ToString();

            _dbContext.Entry(flight)
                .Property(f => f.LastModified)
                .OriginalValue = flightInformation.LastModified;

            await _dbContext.SaveChangesAsync(cancellationToken);

            flightInformation.LastModified = flight.LastModified;

            return new UpdateFlightCommandResponse
            {
                FlightInformation = flightInformation
            };
        }
    }
}
