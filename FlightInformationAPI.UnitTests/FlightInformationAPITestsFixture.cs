using FlightInformation.API.Controllers;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NodaTime;

namespace FlightInformationAPI.UnitTests
{
    public class FlightInformationAPITestsFixture : IDisposable
    {
        private bool _disposed;
        public ApplicationDbContext Database { get; }

        public FlightInformationAPITestsFixture()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            Database = new ApplicationDbContext(dbContextOptions, Mock.Of<IClock>());

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            List<FlightInfo> entities =
                [
                    new FlightInfo
                    {
                        Id = 1,
                        Airline = "Air New Zealand",
                        FlightNumber = "AI101",
                        DepartureAirport = "CHC",
                        ArrivalAirport = "SYD",
                        DepartureTime = new DateTime(2024, 11, 20),
                        ArrivalTime = new DateTime(2024, 11, 21),
                        Status = nameof(FlightDetailStatus.Cancelled),
                        LastModified = new DateTimeOffset(2024, 11, 25, 9, 45, 0, TimeSpan.Zero)
                    },
                    new FlightInfo
                    {
                        Id = 2,
                        Airline = "Virgin Australia",
                        FlightNumber = "VI102",
                        DepartureAirport = "AKL",
                        ArrivalAirport = "DXB",
                        DepartureTime = new DateTime(2024, 11, 20),
                        ArrivalTime = new DateTime(2024, 11, 21),
                        Status = nameof(FlightDetailStatus.Delayed),
                        LastModified = new DateTimeOffset(2024, 11, 25, 9, 45, 0, TimeSpan.Zero)
                    },
                    new FlightInfo
                    {
                        Id = 3,
                        Airline = "Qantas",
                        FlightNumber = "QA103",
                        DepartureAirport = "NPE",
                        ArrivalAirport = "FJI",
                        DepartureTime = new DateTime(2024, 11, 20),
                        ArrivalTime = new DateTime(2024, 11, 21),
                        Status = nameof(FlightDetailStatus.Landed),
                        LastModified = new DateTimeOffset(2024, 11, 25, 9, 45, 0, TimeSpan.Zero)
                    }
                ];

            Database.FlightInfo.AddRange(entities);
            Database.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Database.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
