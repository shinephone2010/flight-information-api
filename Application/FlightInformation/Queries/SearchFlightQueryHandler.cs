using FlightInformation.API.Controllers;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.FlightInformation.Queries
{
    public record SearchFlightsQueryRequest(SearchKeys SearchKeys) : IRequest<SearchFlightQueryResponse>;
    public record SearchFlightQueryResponse
    {
        public IReadOnlyList<FlightDetail> Flights { get; set; } = [];
    }

    public class SearchFlightQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<SearchFlightsQueryRequest, SearchFlightQueryResponse>
    {
        private readonly IApplicationDbContext _dbContext = dbContext;

        public async Task<SearchFlightQueryResponse> Handle(SearchFlightsQueryRequest request, CancellationToken cancellationToken)
        {
            IQueryable<FlightInfo> query = _dbContext.FlightInfo.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchKeys.Airline))
            {
                query = query.Where(f => f.Airline.Equals(request.SearchKeys.Airline, StringComparison.InvariantCultureIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchKeys.DepartureAirport))
            {
                query = query.Where(f => f.DepartureAirport.Equals(request.SearchKeys.DepartureAirport, StringComparison.InvariantCultureIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchKeys.ArrivalAirport))
            {
                query = query.Where(f => f.ArrivalAirport.Equals(request.SearchKeys.ArrivalAirport, StringComparison.InvariantCultureIgnoreCase));
            }

            if (request.SearchKeys.FromDate != default)
            {
                query = query.Where(f => f.DepartureTime.Date >= request.SearchKeys.FromDate.Date);
            }

            if (request.SearchKeys.ToDate != default)
            {
                query = query.Where(f => f.ArrivalTime.Date < request.SearchKeys.ToDate.Date);
            }

            var flightInfoList = await query.ToListAsync(cancellationToken);

            return new SearchFlightQueryResponse
            {
                Flights = [.. flightInfoList.Select(f => f.MapToFlightObject())]
            };

        }
    }
}
