using CM.TableNow.Reservations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CM.TableNow.Reservations.Data.Configurations;

internal sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.TimeSlotId).IsRequired();
        builder.Property(r => r.PartySize).IsRequired();
        builder.Property(r => r.Status).IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.TimeSlotId);
    }
}
