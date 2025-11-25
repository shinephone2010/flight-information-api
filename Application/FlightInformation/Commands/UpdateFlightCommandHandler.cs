using FlightInformation.API.Controllers;
using Infrastructure.Persistence;
using MediatR;

namespace Application.FlightInformation.Commands
{
    public record UpdateFlightCommandRequest(int Id, FlightDetail FlightDetail)
        : IRequest<UpdateFlightCommandResponse>;

    public class UpdateFlightCommandResponse()
    {
        public FlightDetail? FlightDetail { get; set; }
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
                    FlightDetail = null
                };
            }

            flightInfo.Airline = request.FlightDetail.Airline;
            flightInfo.FlightNumber = request.FlightDetail.FlightNumber;
            flightInfo.DepartureAirport = request.FlightDetail.DepartureAirport;
            flightInfo.ArrivalAirport = request.FlightDetail.ArrivalAirport;
            flightInfo.DepartureTime = request.FlightDetail.DepartureTime.DateTime;
            flightInfo.ArrivalTime = request.FlightDetail.ArrivalTime.DateTime;
            flightInfo.Status = request.FlightDetail.Status.ToString();

            // Ensures the flight is updated only if both Id and LastModified match the current values in the database.
            // If another process has already updated this flight, the LastModified value will differ, and SaveChangesAsync
            // will detect the concurrency conflict and throw a DbUpdateConcurrencyException.
            _dbContext.Entry(flightInfo)
                .Property(f => f.LastModified)
                .OriginalValue = flightInfo.LastModified.AddHours(1);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new UpdateFlightCommandResponse
            {
                FlightDetail = request.FlightDetail
            };
        }
    }
}
