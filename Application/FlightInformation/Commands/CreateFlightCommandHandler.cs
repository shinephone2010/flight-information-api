using FlightInformation.API.Controllers;
using Infrastructure.Persistence;
using MediatR;
using FlightInfo = Infrastructure.Persistence.Entities.FlightInfo;

namespace Application.FlightInformation.Commands
{
    public record CreateFlightCommandRequest(Flight Flight) : IRequest<int>;

    public class CreateFlightCommandHandler(IApplicationDbContext dbContext)
        : IRequestHandler<CreateFlightCommandRequest, int>
    {
        private readonly IApplicationDbContext _dbContext = dbContext;

        public async Task<int> Handle(CreateFlightCommandRequest request, CancellationToken cancellationToken)
        {
            var flightInformation = request.Flight;

            var newFlight = new FlightInfo 
            {
                Airline = flightInformation.Airline,
                FlightNumber = flightInformation.FlightNumber,
                DepartureAirport = flightInformation.DepartureAirport,
                ArrivalAirport = flightInformation.ArrivalAirport,
                DepartureTime = flightInformation.DepartureTime.DateTime,
                ArrivalTime = flightInformation.ArrivalTime.DateTime,
                Status = flightInformation.Status.ToString()
            };

            _dbContext.FlightInfo.Add(newFlight);

            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
