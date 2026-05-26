using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace cloud_infrastructure.Models
{
    public class ApplicationDbContext : IdentityDbContext<Developer>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Developer> Developers { get; set; }
        public DbSet<ServerInstance> ServerInstances { get; set; }
        public DbSet<SoftwarePackage> SoftwarePackages { get; set; }
        public DbSet<ServerSoftware> ServerSoftwares { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ServerSoftware>()
                .HasKey(ss => new { ss.ServerInstanceId, ss.SoftwarePackageId });
        }
    }
}
