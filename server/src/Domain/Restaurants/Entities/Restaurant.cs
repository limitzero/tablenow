using CM.TableNow.Restaurants.Domain.ValueObjects;

namespace CM.TableNow.Restaurants.Domain.Entities;

public sealed class Restaurant
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Cuisine { get; set; }
    public required Address Address { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public ICollection<TimeSlot> TimeSlots { get; set; } = [];
}
