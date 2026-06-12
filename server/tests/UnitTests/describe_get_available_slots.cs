using CM.TableNow.Restaurants.Data;
using CM.TableNow.Restaurants.Data.Queries.GetAvailableSlots;
using CM.TableNow.Restaurants.Domain.Entities;
using CM.TableNow.Restaurants.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace describe_get_available_slots;

public class when_party_size_exceeds_remaining_capacity : IAsyncLifetime
{
    private RestaurantsDbContext _db = null!;
    private Guid _restaurantId;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<RestaurantsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _db = new RestaurantsDbContext(options);
        _restaurantId = Guid.NewGuid();

        _db.Restaurants.Add(new Restaurant
        {
            Id = _restaurantId,
            Name = "Test Restaurant",
            Cuisine = "Italian",
            Address = new Address("1 Main St", "Springfield", "IL", "62701")
        });

        _db.TimeSlots.Add(new TimeSlot
        {
            Id = Guid.NewGuid(),
            RestaurantId = _restaurantId,
            StartTime = new DateTimeOffset(2026, 6, 20, 18, 0, 0, TimeSpan.Zero),
            TotalCapacity = 10,
            RemainingCapacity = 2
        });

        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    [Fact]
    public async Task it_should_not_return_the_slot()
    {
        var handler = new GetAvailableSlotsQueryHandler(_db);

        var result = await handler.Handle(
            new GetAvailableSlotsQuery(_restaurantId, new DateOnly(2026, 6, 20), 4),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}

public class when_slot_has_sufficient_capacity : IAsyncLifetime
{
    private RestaurantsDbContext _db = null!;
    private Guid _restaurantId;
    private Guid _slotId;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<RestaurantsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _db = new RestaurantsDbContext(options);
        _restaurantId = Guid.NewGuid();
        _slotId = Guid.NewGuid();

        _db.Restaurants.Add(new Restaurant
        {
            Id = _restaurantId,
            Name = "Test Restaurant",
            Cuisine = "Italian",
            Address = new Address("1 Main St", "Springfield", "IL", "62701")
        });

        _db.TimeSlots.Add(new TimeSlot
        {
            Id = _slotId,
            RestaurantId = _restaurantId,
            StartTime = new DateTimeOffset(2026, 6, 20, 18, 0, 0, TimeSpan.Zero),
            TotalCapacity = 10,
            RemainingCapacity = 6
        });

        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    [Fact]
    public async Task it_should_return_the_slot()
    {
        var handler = new GetAvailableSlotsQueryHandler(_db);

        var result = await handler.Handle(
            new GetAvailableSlotsQuery(_restaurantId, new DateOnly(2026, 6, 20), 4),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data![0].SlotId.Should().Be(_slotId);
        result.Data![0].RemainingCapacity.Should().Be(6);
    }
}
