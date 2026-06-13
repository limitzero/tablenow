using CM.TableNow.Restaurants.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CM.TableNow.Restaurants.Data.Configurations;

internal sealed class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
{
    public void Configure(EntityTypeBuilder<TimeSlot> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.RestaurantId).IsRequired();
        builder.Property(t => t.StartTime).IsRequired();
        builder.Property(t => t.TotalCapacity).IsRequired();
        builder.Property(t => t.RemainingCapacity).IsRequired();

        builder.Property(t => t.RowVersion)
            .IsRowVersion();

        builder.HasIndex(t => new { t.RestaurantId, t.StartTime });
    }
}
