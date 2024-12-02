using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
            modelBuilder.Entity<Grievance>()
                .Property(g => g.FilesUploaded)
                .IsRequired(false);

            modelBuilder.Entity<PoliceOfficer>(entity =>
            {
                // Configure OfficerId as the primary key
                entity.HasKey(o => o.OfficerId);

                // Configure Email as unique
                entity.HasIndex(o => o.Email)
                      .IsUnique()
                      .HasDatabaseName("IX_PoliceOfficers_Email");

                // Configure OfficerName length limit (in case it's not defined in the model)
                entity.Property(o => o.OfficerName)
                      .HasMaxLength(100)
                      .IsRequired();

                // Configure OfficerDesignation length limit
                entity.Property(o => o.OfficerDesignation)
                      .HasMaxLength(100)
                      .IsRequired();

                // Configure Password length limit
                entity.Property(o => o.Password)
                      .HasMaxLength(255);

                // Configure Photo as a nullable varbinary
                entity.Property(o => o.Photo)
                      .IsRequired(false); // This is nullable, like in the DB schema

                // Set the relationship with Departments (1-to-many)
                entity.HasOne(o => o.Department) // Assuming you have a `Department` class
                      .WithMany(d => d.PoliceOfficers) // If one department has many officers
                      .HasForeignKey(o => o.DeptId)
                      .OnDelete(DeleteBehavior.Restrict); // On delete restricts or cascades depending on your needs

                // Configure the relationship for SuperiorId (self-referencing)
                entity.HasOne(o => o.Superior)
                      .WithMany() // Many officers can have the same superior (self-referencing relationship)
                      .HasForeignKey(o => o.SuperiorId)
                      .OnDelete(DeleteBehavior.SetNull); // Or Restrict, depending on the logic

                modelBuilder.Entity<Grievance>()
                       .Property(g => g.TimeLine)
                       .HasColumnType("nvarchar(max)");
            });
        }
        public DbSet<Scheme> GovSchemes { get; set; }
        public DbSet<Eligibility> SchemeEligibilities { get; set; }

        public DbSet<Department> Departments { get; set; } // Table for Departments
        public DbSet<Service> Services { get; set; }       // Table for Services

        public DbSet<ServiceApplication> ServiceApplications { get; set; }

        public DbSet<Grievance> DGrievances { get; set; }
        public DbSet<PoliceOfficer> PoliceOfficers { get; set; }
    }
}
