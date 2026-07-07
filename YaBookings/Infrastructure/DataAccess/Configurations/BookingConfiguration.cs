using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();
        builder.Property(b => b.EventId).IsRequired();
        builder.Property(b => b.UserId).IsRequired();
        builder.Property(b => b.Status).IsRequired().HasConversion<string>();
        builder.Property(b => b.CreatedAt).IsRequired();
    }
}