using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YaEvents.Domain;

namespace YaEvents.Infrastructure.DataAccess.Configurations;

public class ProcessedBookingConfiguration : IEntityTypeConfiguration<ProcessedBooking>
{
    public void Configure(EntityTypeBuilder<ProcessedBooking> builder)
    {
        builder.ToTable("ProcessedBookings");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.BookingId).IsRequired();
        builder.Property(e => e.EventId).IsRequired();
        builder.Property(e => e.SeatsCount).IsRequired();
        builder.Property(e => e.ProcessedAt).IsRequired();

        builder.HasIndex(e => e.BookingId)
                  .IsUnique()
                  .HasDatabaseName("IX_processed_bookings_booking_id");
    }
}