namespace CM.TableNow.Restaurants.Domain.Entities;

public sealed class TimeSlot
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public int TotalCapacity { get; set; }
    public int RemainingCapacity { get; set; }
    public byte[]? RowVersion { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
}
