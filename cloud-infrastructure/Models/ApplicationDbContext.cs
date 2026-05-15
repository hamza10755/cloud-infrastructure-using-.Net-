using Microsoft.EntityFrameworkCore;

namespace cloud_infrastructure.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Developer> Developers { get; set; }
        public DbSet<ServerInstance> ServerInstances { get; set; }
        public DbSet<SoftwarePackage> SoftwarePackages { get; set; }
        public DbSet<ServerSoftware> ServerSoftwares { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServerSoftware>()
                .HasKey(ss => new { ss.ServerInstanceId, ss.SoftwarePackageId });
        }
    }
}
