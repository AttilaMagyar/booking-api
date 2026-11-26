using BookingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Data;

public class BookingDbContext(DbContextOptions<BookingDbContext> options) : DbContext(options)
{
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.ToTable("resources");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).HasMaxLength(150).IsRequired();
            entity.Property(r => r.Category).HasMaxLength(80).IsRequired();
            entity.HasIndex(r => r.Name).IsUnique();
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("bookings");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.BookedByEmail).HasMaxLength(160).IsRequired();
            entity.Property(b => b.Notes).HasMaxLength(300);
            entity.Property(b => b.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasIndex(b => new { b.ResourceId, b.StartAt, b.EndAt });

            entity.HasOne(b => b.Resource)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.ResourceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
