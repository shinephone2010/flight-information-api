using FlightInformation.API.Controllers;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.FlightInformation.Queries
{
    public record GetAllFlightsQueryRequest() : IRequest<GetAllFlightsQueryResponse>;

    public class GetAllFlightsQueryResponse
    {
        public IReadOnlyList<FlightDetail>? AllFlights { get; set; }
    }

    public class GetAllFlightsQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetAllFlightsQueryRequest, GetAllFlightsQueryResponse>
    {
        private readonly IApplicationDbContext _dbContext = dbContext;

        public async Task<GetAllFlightsQueryResponse> Handle(GetAllFlightsQueryRequest request, CancellationToken cancellationToken)
        {
            var allFlights = await _dbContext.FlightInfo.AsNoTracking().ToListAsync(cancellationToken);

            return new GetAllFlightsQueryResponse
            {
                AllFlights = [.. allFlights.Select(f => f.MapToFlightObject())]
            };
        }
    }

}
