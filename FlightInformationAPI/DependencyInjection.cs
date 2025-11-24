using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlightInformationAPI
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("FlightInformationDb"));

            // 👇 This line tells DI how to build IApplicationDbContext
            services.AddScoped<IApplicationDbContext>(sp =>
                sp.GetRequiredService<ApplicationDbContext>());

            return services;
        }
    }
}
