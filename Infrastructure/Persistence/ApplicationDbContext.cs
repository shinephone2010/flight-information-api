using Infrastructure.Extensions;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using System.Reflection.Metadata;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly IClock _clock;
        public virtual DbSet<FlightInformation> FlightInformation { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IClock clock)
            : base(options)
        {
            _clock = clock;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var flightInfo = modelBuilder.Entity<FlightInformation>();

            flightInfo.Property(f => f.LastModified)
                .IsConcurrencyToken()   // tells EF to use this in concurrency checks
                .HasPrecision(3);       // optional: reduce precision to avoid SQL vs .NET mismatch

            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Set LastModified for updated entities
            var utc = _clock.GetUtcDateTimeOffsetToUnixTimeSeconds();

            foreach (var entry in ChangeTracker.Entries<FlightInformation>())
            {
                if (entry.State == EntityState.Modified)
                {
                    // Only update LastModified on real updates
                    entry.Entity.LastModified = utc;
                }
                else if (entry.State == EntityState.Added)
                {
                    // Initialize on insert
                    entry.Entity.LastModified = utc;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

    }
}
