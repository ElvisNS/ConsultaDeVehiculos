using Microsoft.EntityFrameworkCore;
using VehicleRegistryAPI.Entities;

namespace VehicleRegistryAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Person> Persons { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Property Configuration

            #region User

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(u => u.Id);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(u => u.UserName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(u => u.PasswordHash)
                      .IsRequired();

                entity.Property(u => u.IsActive)
                      .HasDefaultValue(true);

                entity.Property(u => u.CreatedAt)
                      .IsRequired();
            });

            #endregion

            #region Role

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.ToTable("Roles");

                entity.HasKey(r => r.Id);

                entity.Property(r => r.Name)
                      .IsRequired()
                      .HasMaxLength(50);
            });

            #endregion

            #region UserRoles

            modelBuilder.Entity<UserRoles>(entity =>
            {
                entity.ToTable("UserRoles");

                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.HasOne(ur => ur.Users)
                      .WithMany(u => u.UserRoless)
                      .HasForeignKey(ur => ur.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                      .WithMany(r => r.UserRoless)
                      .HasForeignKey(ur => ur.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion

            #region Person

            modelBuilder.Entity<Person>(entity =>
            {
                entity.ToTable("Persons");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.NationalId)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(p => p.FullName)
                      .IsRequired()
                      .HasMaxLength(100);

                // NationalId único
                entity.HasIndex(p => p.NationalId)
                      .IsUnique();

                entity.Property(p => p.CreatedAt)
                       .IsRequired();

                entity.Property(p => p.UpdatedAt);

                entity.Property(p => p.DeactivatedAt);
            });

            #endregion

            #region Car

            modelBuilder.Entity<Car>(entity =>
            {
                entity.ToTable("Cars");

                entity.HasKey(c => c.Id);

                entity.Property(c => c.PlateNumber)
                      .IsRequired()
                      .HasMaxLength(20);
                entity.HasIndex(c => c.PlateNumber)
                       .IsUnique();

                entity.Property(c => c.Brand)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(c => c.Model)
                      .IsRequired()
                      .HasMaxLength(50);

                // Relación 1:N
                entity.HasOne(c => c.Persons)// Un carro tiene UNA persona
                      .WithMany(p => p.Cars)// Una persona tiene MUCHOS carros
                      .HasForeignKey(c => c.PersonId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(c => c.CreatedAt)
                       .IsRequired();

                entity.Property(c => c.UpdatedAt);

                entity.Property(c => c.DeactivatedAt);
            });

            #endregion

            #endregion
        }

    }
}
