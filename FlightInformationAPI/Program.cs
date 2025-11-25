using Application.FlightInformation.Commands;
using Application.FlightInformation.Validation;
using FlightInformation.API.Controllers;
using FlightInformationAPI;
using FluentValidation;
using Infrastructure.Helpers;
using Infrastructure.Persistence;
using NodaTime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure();
builder.Services.AddSingleton<IClock>(SystemClock.Instance);
builder.Services.AddScoped<IValidator<Flight>, FlightValidator>();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssemblyContaining<UpdateFlightCommandHandler>();
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var clock = scope.ServiceProvider.GetRequiredService<IClock>();
    await CsvSeeder.SeedFlightDataFromResourceAsync(db, clock);
}

app.Run();
