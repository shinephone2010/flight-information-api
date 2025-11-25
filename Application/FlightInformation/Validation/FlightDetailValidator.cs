using FlightInformation.API.Controllers;
using FluentValidation;

namespace Application.FlightInformation.Validation
{
    public class FlightDetailValidator : AbstractValidator<FlightDetail>
    {
        public FlightDetailValidator()
        {
            RuleFor(x => x.Airline)
                .NotNull().NotEmpty().WithMessage("Airline is required.")
                .MaximumLength(30).WithMessage("Airline must be at most 30 characters.");

            RuleFor(x => x.FlightNumber)
                .NotEmpty().WithMessage("Flight number is required.")
                .MaximumLength(5).WithMessage("Flight number must be at most 10 characters.");

            RuleFor(x => x.DepartureAirport)
                .NotNull().NotEmpty().WithMessage("Departure airport is required.")
                .Length(3).WithMessage("Departure airport must be a 3-letter IATA code.");

            RuleFor(x => x.ArrivalAirport)
                .NotNull().NotEmpty().WithMessage("Arrival airport is required.")
                .Length(3).WithMessage("Arrival airport must be a 3-letter IATA code.")
                .NotEqual(x => x.DepartureAirport).WithMessage("Arrival airport must be different from departure airport.");

            RuleFor(x => x.ArrivalTime)
                .GreaterThan(x => x.DepartureTime)
                .WithMessage("Arrival time must be after departure time.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Status must be a valid flight status.");
        }
    }
}
