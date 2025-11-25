using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlightInformationAPI.IntegrationTests
{
    /// <summary>
    /// Custom factory that swaps the real database for an in-memory database
    /// so we can exercise the full pipeline (controllers + MediatR + EF).
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 1. Remove the existing ApplicationDbContext registration
                var dbContextDescriptor = services
                    .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (dbContextDescriptor is not null)
                {
                    services.Remove(dbContextDescriptor);
                }

                // 2. Remove existing IApplicationDbContext registration
                var iAppDbContextDescriptor = services
                    .SingleOrDefault(d => d.ServiceType == typeof(IApplicationDbContext));

                if (iAppDbContextDescriptor is not null)
                {
                    services.Remove(iAppDbContextDescriptor);
                }

                // 3. Re-add ApplicationDbContext with InMemory provider for tests
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("FlightInformationTestsDb");
                });

                // 4. Re-add IApplicationDbContext mapping pointing to the in-memory context
                services.AddScoped<IApplicationDbContext>(sp =>
                    sp.GetRequiredService<ApplicationDbContext>());

                // 5. Build provider and ensure DB is created
                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }
}