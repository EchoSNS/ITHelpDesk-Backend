using ITHelpDesk.Domain;
using ITHelpDesk.Domain.Department;
using ITHelpDesk.Domain.Ticket;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Data
{
    public class HelpDeskDbContext : IdentityDbContext<ApplicationUser>
    {
        public HelpDeskDbContext(DbContextOptions<HelpDeskDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<TicketView> TicketViews { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<SubDepartment> SubDepartments { get; set; }
        public DbSet<Position> Positions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define explicit table names
            modelBuilder.Entity<Ticket>().ToTable("Tickets");
            modelBuilder.Entity<TicketComment>().ToTable("TicketComments");
            modelBuilder.Entity<TicketView>().ToTable("TicketViews");
            modelBuilder.Entity<Department>().ToTable("Departments");
            modelBuilder.Entity<SubDepartment>().ToTable("SubDepartments");
            modelBuilder.Entity<Position>().ToTable("Positions");

            // Configure Ticket relationships
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasOne(t => t.Submitter)
                    .WithMany(u => u.SubmittedTickets)
                    .HasForeignKey(t => t.SubmitterId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.AssignedTo)
                    .WithMany(u => u.AssignedTickets)
                    .HasForeignKey(t => t.AssignedToId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(t => t.CreatedAt)
                    .ValueGeneratedOnAdd();

                // Remove default value for ClosedAt - it should only be set when a ticket is actually closed
                entity.Property(t => t.ClosedAt)
                    .IsRequired(false); // Make nullable to indicate it's not closed yet

                entity.Property(t => t.UpdatedAt)
                    .ValueGeneratedOnAddOrUpdate();
            });

            // Configure TicketView
            modelBuilder.Entity<TicketView>(entity =>
            {
                entity.HasKey(tv => tv.Id);

                entity.Property(t => t.ViewedAt)
                    .ValueGeneratedOnAdd();

                entity.HasOne(tv => tv.Ticket)
                    .WithMany(t => t.TicketViews)
                    .HasForeignKey(tv => tv.TicketId);

                entity.HasOne(tv => tv.User)
                    .WithMany(u => u.TicketViews)
                    .HasForeignKey(tv => tv.UserId);
            });

            // Configure TicketComment relationships
            modelBuilder.Entity<TicketComment>(entity =>
            {
                entity.HasOne(c => c.Ticket)
                    .WithMany(t => t.Comments)
                    .HasForeignKey(c => c.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                    .WithMany(u => u.TicketComments)
                    .HasForeignKey(c => c.UserId);

                entity.Property(c => c.CreatedAt)
                    .ValueGeneratedOnAdd();
            });

            // Department hierarchy configurations
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasOne(d => d.DepartmentManager)
                    .WithMany(u => u.ManagedDepartments)
                    .HasForeignKey(d => d.DepartmentManagerId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Add subdepartments relationship
                entity.HasMany(d => d.SubDepartments)
                    .WithOne(sd => sd.Department)
                    .HasForeignKey(sd => sd.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SubDepartment>(entity =>
            {
                entity.HasOne(sd => sd.SubDepartmentManager)
                    .WithMany(u => u.ManagedSubDepartments)
                    .HasForeignKey(sd => sd.SubDepartmentManagerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(sd => sd.Department)
                    .WithMany(d => d.SubDepartments)
                    .HasForeignKey(sd => sd.DepartmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Add positions relationship
                entity.HasMany(sd => sd.Positions)
                    .WithOne(p => p.SubDepartment)
                    .HasForeignKey(p => p.SubDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Position>(entity =>
            {
                entity.HasOne(p => p.SubDepartment)
                    .WithMany(sd => sd.Positions)
                    .HasForeignKey(p => p.SubDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // User entity configurations
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(u => u.Department)
                    .WithMany(d => d.Users)
                    .HasForeignKey(u => u.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(u => u.SubDepartment)
                    .WithMany(sd => sd.Users)
                    .HasForeignKey(u => u.SubDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.Position)
                    .WithMany(p => p.Users)
                    .HasForeignKey(u => u.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Indexes for performance improvement
            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.Status);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.Priority);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.CreatedAt);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.DepartmentId);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.SubDepartmentId);
        }
    }
}