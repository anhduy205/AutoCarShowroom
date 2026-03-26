using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Models
{
    public class ShowroomDbContext : DbContext
    {
        public ShowroomDbContext(DbContextOptions<ShowroomDbContext> options)
            : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(order => order.OrderCode)
                    .IsUnique();

                entity.Property(order => order.TotalAmount)
                    .HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(item => item.UnitPrice)
                    .HasColumnType("decimal(18,2)");

                entity.HasOne(item => item.Order)
                    .WithMany(order => order.Items)
                    .HasForeignKey(item => item.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(item => item.Car)
                    .WithMany()
                    .HasForeignKey(item => item.CarId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
