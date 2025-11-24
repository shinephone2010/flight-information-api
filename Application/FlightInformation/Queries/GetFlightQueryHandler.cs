using FlightInformation.API.Controllers;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using MediatR;

namespace Application.FlightInformation.Queries
{
    public record GetFlightQueryRequest(int Id) : IRequest<GetFlightQueryResponse>;

    public class GetFlightQueryResponse
    {
        public Flight? Flight { get; set; }
    }

    public class GetFlightQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetFlightQueryRequest, GetFlightQueryResponse>
    {
        private readonly IApplicationDbContext _dbContext = dbContext;

        public async Task<GetFlightQueryResponse> Handle(GetFlightQueryRequest request, CancellationToken cancellationToken)
        {
            var flightInfo = await _dbContext.FlightInfo
               .FindAsync([request.Id], cancellationToken);

            return new GetFlightQueryResponse
            {
                Flight = flightInfo != null
                    ? flightInfo.MapToFlightObject()
                    : null
            };

        }
    }

}
