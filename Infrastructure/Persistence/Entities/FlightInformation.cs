namespace Infrastructure.Persistence.Entities
{
    public class FlightInformation
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; } = null!;
        public string Airline { get; set; } = null!;
        public string DepartureAirport { get; set; } = null!;
        public string ArrivalAirport { get; set; } = null!;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string Status { get; set; } = null!;
    }
}
