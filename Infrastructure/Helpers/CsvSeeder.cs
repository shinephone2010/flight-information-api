using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using NodaTime;

namespace Infrastructure.Helpers
{
    public static class CsvSeeder
    {
        public static async Task SeedFlightDataFromResourceAsync(
            IApplicationDbContext dbContext,
            IClock clock,
            CancellationToken cancellationToken = default)
        {
            var flightData = Resource.FlightInformation;
            if (string.IsNullOrWhiteSpace(flightData))
            {
                return;
            }

            using var stringReader = new StringReader(flightData);

            var headerLine = await stringReader.ReadLineAsync(cancellationToken);
            if (headerLine is null)
            {
                return;
            }

            List<FlightInformation> flights = [];
            string? line;
            while ((line = await stringReader.ReadLineAsync(cancellationToken)) is not null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var columns = line.Split(',');

                if (columns.Length != 8)
                {
                    continue;
                }

                flights.Add(
                    new FlightInformation
                    {
                        Id = Convert.ToInt32(columns[0].Trim()),
                        FlightNumber = columns[1].Trim(),
                        Airline = columns[2].Trim(),
                        DepartureAirport = columns[3].Trim(),
                        ArrivalAirport = columns[4].Trim(),
                        DepartureTime = Convert.ToDateTime(columns[5].Trim()),
                        ArrivalTime = Convert.ToDateTime(columns[6].Trim()),
                        Status = columns[7].Trim(),
                        LastModified = clock.GetUtcDateTimeOffsetToUnixTimeSeconds()
                    });
            }

            if (flights.Count > 0)
            {
                await dbContext.FlightInformation.AddRangeAsync(flights, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

        }
    }
}
