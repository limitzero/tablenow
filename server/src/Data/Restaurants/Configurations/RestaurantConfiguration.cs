using CM.TableNow.Restaurants.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CM.TableNow.Restaurants.Data.Configurations;

internal sealed class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Cuisine).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Description).HasMaxLength(2000);
        builder.Property(r => r.ThumbnailUrl).HasMaxLength(500);

        builder.OwnsOne(r => r.Address, a =>
        {
            a.Property(x => x.Street).IsRequired().HasMaxLength(300);
            a.Property(x => x.City).IsRequired().HasMaxLength(100);
            a.Property(x => x.Region).IsRequired().HasMaxLength(100);
            a.Property(x => x.PostalCode).IsRequired().HasMaxLength(20);
        });

        builder.HasMany(r => r.TimeSlots)
            .WithOne(t => t.Restaurant)
            .HasForeignKey(t => t.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
