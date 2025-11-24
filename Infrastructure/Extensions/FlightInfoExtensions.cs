using FlightInformation.API.Controllers;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Extensions
{
    public static class FlightInfoExtensions
    {
        public static Flight MapToFlightObject(this FlightInfo flightInfo)
        {
            var (id, flightNumber, airline, departureAirport, arrivalAirport, departureTime, arrivalTime, status, lastModified) = flightInfo;
            return new Flight
            {
                Id = id,
                FlightNumber = flightNumber,
                Airline = airline,
                DepartureAirport = departureAirport,
                ArrivalAirport = arrivalAirport,
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime,
                Status = status,
                LastModified = lastModified
            };
        }
    }
}
