namespace Application.FlightInformation.Models
{
    public class FlightInformationDto
    {
        public required string FlightNumber { get; set; }
        public required string Airline { get; set; }
        public required string DepartureAirport { get; set; }
        public required string ArrivalAirport { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public required string Status { get; set; }
        public DateTime LastModified { get; set; }
    }
}
