using FlightInformation.API.Controllers;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using MediatR;

namespace Application.FlightInformation.Commands
{
    public record DeleteFlightCommandRequest(int Id) : IRequest<DeleteFlightCommandResponse>;

    public class DeleteFlightCommandResponse
    {
        public FlightDetail? FlightDetail { get; set; }
    }

    public class DeleteFlightCommandHandler(IApplicationDbContext dbContext)
        : IRequestHandler<DeleteFlightCommandRequest, DeleteFlightCommandResponse>
    {
        private readonly IApplicationDbContext _dbContext = dbContext;

        public async Task<DeleteFlightCommandResponse> Handle(DeleteFlightCommandRequest request, CancellationToken cancellationToken)
        {
            var flightInfo = await _dbContext.FlightInfo
                .FindAsync([request.Id], cancellationToken);

            if (flightInfo == null)
            {
                return new DeleteFlightCommandResponse
                {
                    FlightDetail = null
                };
            }

            _dbContext.FlightInfo.Remove(flightInfo);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new DeleteFlightCommandResponse
            {
                FlightDetail = flightInfo.MapToFlightObject()
            };
        }
    }
}
