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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            #region Property Configuration

            #region User

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(u => u.Id);

                entity.Property(u => u.UserName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(u => u.PasswordHash)
                      .IsRequired();
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
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(c => c.PlateNumber)
                      .IsUnique();
            });

            #endregion
        }

            #endregion

    }
}
