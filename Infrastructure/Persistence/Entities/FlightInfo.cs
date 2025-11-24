using FlightInformation.API.Controllers;

namespace Infrastructure.Persistence.Entities
{
    public class FlightInfo
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; } = null!;
        public string Airline { get; set; } = null!;
        public string DepartureAirport { get; set; } = null!;
        public string ArrivalAirport { get; set; } = null!;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string Status { get; set; } = null!;
        public DateTimeOffset LastModified { get; set; }

        public void Deconstruct(
            out int id,
            out string flightNumber,
            out string airline,
            out string departureAirport,
            out string arrivalAirport,
            out DateTimeOffset departureTime,
            out DateTimeOffset arrivalTime,
            out FlightStatus status,
            out DateTimeOffset lastModified)
        {
            id = Id;
            flightNumber = FlightNumber;
            airline = Airline;
            departureAirport = DepartureAirport;
            arrivalAirport = ArrivalAirport;
            departureTime = new DateTimeOffset(DepartureTime);
            arrivalTime = new DateTimeOffset(ArrivalTime);
            status = Enum.Parse<FlightStatus>(Status);
            lastModified = LastModified;
        }
    }
}
