using FlightInformation.API.Controllers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace FlightInformationAPI.IntegrationTests
{
    public class FlightInformationControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public FlightInformationControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetFlights_Returns_Ok_And_SeededFlights()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/flights");

            // Assert
            response.EnsureSuccessStatusCode();

            var flights = await response.Content.ReadFromJsonAsync<ICollection<FlightDetail>>();

            flights.Should().NotBeNull();
            flights!.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetFlight_Returns_NotFound_When_Id_Does_Not_Exist()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/flights/999999"); // some non-existing id

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

            error.Should().NotBeNull();
            error!.Error.Should().Be("Not Found");
            error.Message.Should().Contain("999999");
        }

        [Fact]
        public async Task GetFlight_Returns_Ok_For_Existing_Flight()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Get all flights
            var allResponse = await client.GetAsync("api/flights");
            allResponse.EnsureSuccessStatusCode();

            var flights = await allResponse.Content.ReadFromJsonAsync<List<FlightDetail>>();
            flights.Should().NotBeNull();
            flights!.Should().NotBeEmpty();

            var existing = flights![0];

            var getResponse = await client.GetAsync($"api/flights/{existing.Id}");

            // Assert
            getResponse.EnsureSuccessStatusCode();
            var returnedFlight = await getResponse.Content.ReadFromJsonAsync<FlightDetail>();

            returnedFlight.Should().NotBeNull();
            returnedFlight!.Should().BeEquivalentTo(existing);
        }

        [Fact]
        public async Task CreateFlight_Returns_BadRequest_When_Model_Is_Invalid()
        {
            // Arrange
            var client = _factory.CreateClient();

            var invalidFlight = new FlightDetail
            {
                Airline = "",
                FlightNumber = "",
                DepartureAirport = "",
                ArrivalAirport = "",
            };

            // Act
            var response = await client.PostAsJsonAsync("api/flights", invalidFlight);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

            error.Should().NotBeNull();
            error!.Error.Should().Be("Validation Failure");
            error.Message.Should().Be("Invalid data format");
            error.Details.Should().NotBeNull();
            error.Details.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CreateFlight_Then_GetFlights_Contains_NewFlight()
        {
            // Arrange
            var client = _factory.CreateClient();

            var departureTime = new DateTime(2025, 6, 20);

            var newFlight = new FlightDetail
            {
                FlightNumber = "INT01",
                Airline = "Test Air",
                DepartureAirport = "AAA",
                ArrivalAirport = "BBB",
                DepartureTime = departureTime,
                ArrivalTime = departureTime.AddHours(3),
                Status = FlightDetailStatus.Scheduled
            };

            // Act: create
            var createResponse = await client.PostAsJsonAsync("api/flights", newFlight);

            // Assert: create OK
            createResponse.EnsureSuccessStatusCode();
            var header = createResponse.Headers.GetValues("Location").First().Should().Be("http://localhost/api/flights/51");
            var createBody = await createResponse.Content.ReadAsStringAsync();
            Assert.Contains("Flight created successfully", createBody);

            // Act: get all flights
            var getResponse = await client.GetAsync("api/flights");
            getResponse.EnsureSuccessStatusCode();

            var flights = await getResponse.Content.ReadFromJsonAsync<List<FlightDetail>>();

            flights.Should().NotBeNull();
            flights!.Should().Contain(f =>
                f.FlightNumber == "INT01"
                && f.Airline == "Test Air"
                && f.DepartureAirport == "AAA"
                && f.ArrivalAirport == "BBB"
                && f.Status == FlightDetailStatus.Scheduled
                && f.DepartureTime == departureTime
                && f.ArrivalTime == departureTime.AddHours(3));
        }

        [Fact]
        public async Task DeleteFlight_Returns_NotFound_For_Unknown_Id()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.DeleteAsync("api/flights/999999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

            error.Should().NotBeNull();
            error!.Error.Should().Be("Not Found");
        }

        [Fact]
        public async Task SearchFlights_Returns_Ok_And_Collection()
        {
            // Arrange
            var client = _factory.CreateClient();

            var response = await client.GetAsync("api/flights/search?airline=TestAir");

            // Assert
            response.EnsureSuccessStatusCode();

            var flights = await response.Content.ReadFromJsonAsync<ICollection<FlightDetail>>();
            flights.Should().NotBeNull();
        }

        [Fact]
        public async Task SearchFlights_Binds_SearchKeys_And_Returns_Ok()
        {
            // Arrange
            var client = _factory.CreateClient();

            var allResponse = await client.GetAsync("/api/flights");
            allResponse.EnsureSuccessStatusCode();

            var flights = await allResponse.Content.ReadFromJsonAsync<List<FlightDetail>>();
            flights.Should().NotBeNull();
            flights!.Should().NotBeEmpty();

            var sample = flights![0];

            var departureDate = sample.DepartureTime.Date;
            var fromDate = departureDate.ToString("yyyy-MM-dd");
            var toDate = departureDate.AddDays(1).ToString("yyyy-MM-dd");

            var url =
                $"/api/flights/search" +
                $"?searchKeys.airline={Uri.EscapeDataString(sample.Airline ?? string.Empty)}" +
                $"&searchKeys.departureAirport={Uri.EscapeDataString(sample.DepartureAirport ?? string.Empty)}" +
                $"&searchKeys.arrivalAirport={Uri.EscapeDataString(sample.ArrivalAirport ?? string.Empty)}" +
                $"&searchKeys.fromDate={Uri.EscapeDataString(fromDate)}" +
                $"&searchKeys.toDate={Uri.EscapeDataString(toDate)}";

            // Act
            var response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();

            var resultFlights = await response.Content.ReadFromJsonAsync<ICollection<FlightDetail>>();

            resultFlights.Should().NotBeNull();
            resultFlights!.Should().Contain(f => f.Id == sample.Id);
        }

        [Fact]
        public async Task UpdateFlight_Returns_BadRequest_When_Model_Invalid()
        {
            // Arrange
            var client = _factory.CreateClient();

            var allResponse = await client.GetAsync("api/flights/1");
            allResponse.EnsureSuccessStatusCode();
            var originalFlightDetail = await allResponse.Content.ReadFromJsonAsync<FlightDetail>();
            originalFlightDetail.Should().NotBeNull();

            var invalidFlightDetail = new FlightDetail
            {
                Id = originalFlightDetail.Id,
                FlightNumber = "Invalid Flight Number",
                Airline = originalFlightDetail.Airline,
                DepartureAirport = "ABC",
                ArrivalAirport = "ABC",
                DepartureTime = originalFlightDetail.DepartureTime,
                ArrivalTime = originalFlightDetail.DepartureTime,
                Status = FlightDetailStatus.Delayed
            };

            // Act
            var response = await client.PutAsJsonAsync($"api/flights/{invalidFlightDetail.Id}", invalidFlightDetail);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

            error.Should().NotBeNull();
            error!.Error.Should().Be("Validation Failure");

            string[] expectedErrorDetails =
                [
                    "FlightNumber: Flight number must be at most 5 characters.",
                    "ArrivalAirport: Arrival airport must be different from departure airport.",
                    "ArrivalTime: Arrival time must be after departure time."
                ];

            error.Details.Should().NotBeNull();
            error.Details.Should().Equal(expectedErrorDetails);
        }

        [Fact]
        public async Task UpdateFlight_Returns_NotFound_When_Id_Does_Not_Exist()
        {
            // Arrange
            var client = _factory.CreateClient();

            var update = new FlightDetail
            {
                Id = 999999,
                FlightNumber = "INT01",
                Airline = "TestAir",
                DepartureAirport = "AAA",
                ArrivalAirport = "BBB",
                DepartureTime = DateTime.UtcNow.AddHours(1),
                ArrivalTime = DateTime.UtcNow.AddHours(3),
                Status = FlightDetailStatus.Scheduled
            };

            // Act
            var response = await client.PutAsJsonAsync("api/flights/999999", update);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
