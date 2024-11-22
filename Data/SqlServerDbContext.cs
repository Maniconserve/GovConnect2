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

            modelBuilder.Entity<Service>()
                .HasOne(s => s.department)          // One service has one department
                .WithMany(d => d.Services)          // A department has many services
                .HasForeignKey(s => s.DeptId);      // Foreign Key is DeptId
            modelBuilder.Entity<Service>()
            .OwnsOne(s => s.FeeDetails, fee =>
            {
                fee.Property(f => f.GovFee).HasColumnName("GovFee");
                fee.Property(f => f.ServiceFee).HasColumnName("ServiceFee");
                fee.Property(f => f.Tax).HasColumnName("Tax");
            });

            modelBuilder.Entity<ServiceApplication>()
                .HasOne<Citizen>() // Assuming Citizen is the related model for UserID
                .WithMany()        // Add the appropriate relationship configuration
                .HasForeignKey(s => s.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceApplication>()
                .HasOne<Service>() // Assuming Service is the related model for ServiceID
                .WithMany()         // Add the appropriate relationship configuration
                .HasForeignKey(s => s.ServiceID)
                .OnDelete(DeleteBehavior.Cascade);
        }
        public DbSet<Scheme> GovSchemes { get; set; }
        public DbSet<Eligibility> SchemeEligibilities { get; set; }

        public DbSet<Department> Departments { get; set; } // Table for Departments
        public DbSet<Service> Services { get; set; }       // Table for Services

        public DbSet<ServiceApplication> ServiceApplications { get; set; }

        public DbSet<Grievance> DGrievances { get; set; }
    }
}
