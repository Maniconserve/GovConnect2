using GovConnect.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace GovConnect.Data
{
    public class SqlServerDbContext : IdentityDbContext<Citizen>
    {
        public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Citizen>()
               .ToTable("Citizens");

            modelBuilder.Entity<Citizen>()
                .Property(u => u.UserName)
                .HasColumnName("FirstName");
        }
        public DbSet<Scheme> GovSchemes { get; set; }
        public DbSet<Eligibility> SchemeEligibilities { get; set; }
    }
}
