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
        public IReadOnlyList<Flight>? AllFlights { get; set; }
    }

    public class GetAllFlightsQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetAllFlightsQueryRequest, GetAllFlightsQueryResponse>
    {
        private readonly IApplicationDbContext _dbContext = dbContext;

        public async Task<GetAllFlightsQueryResponse> Handle(GetAllFlightsQueryRequest request, CancellationToken cancellationToken)
        {
            return new GetAllFlightsQueryResponse
            {
                AllFlights = [..await _dbContext.FlightInfo.AsNoTracking().Select(f => f.MapToFlightObject()).ToListAsync(cancellationToken)]
            };
        }
    }

}
