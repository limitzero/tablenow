using CM.TableNow.Restaurants.Domain.Entities;
using CM.TableNow.Restaurants.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Restaurants.Data.Seed;

/// <summary>
/// Seeds a varied set of demo restaurants spanning multiple cuisines so the reservation
/// flow can be demonstrated end-to-end. Idempotent: skips seeding if any restaurant exists.
/// </summary>
/// <remarks>
/// The <see cref="RestaurantIds"/> GUIDs are stable and hard-coded. The time-slot seeder
/// (task-02) keys its generated slots to these exact IDs, so they must never change.
/// </remarks>
public static class RestaurantDataSeeder
{
    /// <summary>
    /// Stable restaurant identifiers, ordered to match <see cref="BuildRestaurants"/>.
    /// Exposed so the time-slot seeder can attach slots to each restaurant by ID.
    /// </summary>
    public static readonly Guid[] RestaurantIds =
    [
        Guid.Parse("22222222-0000-0000-0000-000000000001"), // La Bella Italia
        Guid.Parse("22222222-0000-0000-0000-000000000002"), // Trattoria Roma
        Guid.Parse("22222222-0000-0000-0000-000000000003"), // Osteria Fiorentina
        Guid.Parse("22222222-0000-0000-0000-000000000004"), // Sakura Garden
        Guid.Parse("22222222-0000-0000-0000-000000000005"), // Nori Sushi House
        Guid.Parse("22222222-0000-0000-0000-000000000006"), // Tokyo Ramen Bar
        Guid.Parse("22222222-0000-0000-0000-000000000007"), // El Mariachi
        Guid.Parse("22222222-0000-0000-0000-000000000008"), // Casa Oaxaca
        Guid.Parse("22222222-0000-0000-0000-000000000009"), // Taqueria del Sol
        Guid.Parse("22222222-0000-0000-0000-00000000000a"), // The Grill House
        Guid.Parse("22222222-0000-0000-0000-00000000000b"), // Liberty Diner
        Guid.Parse("22222222-0000-0000-0000-00000000000c"), // Smokestack BBQ
        Guid.Parse("22222222-0000-0000-0000-00000000000d"), // Maharaja Palace
        Guid.Parse("22222222-0000-0000-0000-00000000000e"), // Spice Route
        Guid.Parse("22222222-0000-0000-0000-00000000000f"), // Tandoori Nights
        Guid.Parse("22222222-0000-0000-0000-000000000010"), // Bombay Bistro
    ];

    public static async Task SeedAsync(RestaurantsDbContext db, CancellationToken ct = default)
    {
        // Idempotency guard: if any restaurant exists the seed has already run, so do nothing.
        if (await db.Restaurants.AnyAsync(ct))
        {
            return;
        }

        db.Restaurants.AddRange(BuildRestaurants());

        await db.SaveChangesAsync(ct);
    }

    // Builds the restaurant records in the same order as RestaurantIds. Kept private so the
    // single source of truth for IDs remains the RestaurantIds array above.
    private static IEnumerable<Restaurant> BuildRestaurants()
    {
        yield return Create(RestaurantIds[0], "La Bella Italia", "Italian",
            new Address("142 Vine Street", "Portland", "OR", "97204"),
            "Handmade pasta and wood-fired pizza in a cozy, candle-lit dining room.");

        yield return Create(RestaurantIds[1], "Trattoria Roma", "Italian",
            new Address("88 Market Avenue", "Seattle", "WA", "98101"),
            "Classic Roman dishes and an extensive regional wine list.");

        yield return Create(RestaurantIds[2], "Osteria Fiorentina", "Italian",
            new Address("17 Harbor Lane", "San Francisco", "CA", "94111"),
            "Tuscan-inspired plates with seasonal produce from local farms.");

        yield return Create(RestaurantIds[3], "Sakura Garden", "Japanese",
            new Address("305 Cherry Boulevard", "Portland", "OR", "97205"),
            "Traditional kaiseki tasting menus and a serene garden patio.");

        yield return Create(RestaurantIds[4], "Nori Sushi House", "Japanese",
            new Address("412 Pearl Street", "Seattle", "WA", "98109"),
            "Omakase sushi crafted from daily-flown fresh catch.");

        yield return Create(RestaurantIds[5], "Tokyo Ramen Bar", "Japanese",
            new Address("66 Lantern Way", "San Francisco", "CA", "94133"),
            "Rich tonkotsu and shoyu ramen simmered for eighteen hours.");

        yield return Create(RestaurantIds[6], "El Mariachi", "Mexican",
            new Address("221 Agave Road", "Austin", "TX", "78701"),
            "Street-style tacos and house margaritas with live mariachi nightly.");

        yield return Create(RestaurantIds[7], "Casa Oaxaca", "Mexican",
            new Address("9 Mole Plaza", "San Diego", "CA", "92101"),
            "Authentic Oaxacan moles and mezcal flights.");

        yield return Create(RestaurantIds[8], "Taqueria del Sol", "Mexican",
            new Address("550 Sunset Drive", "Phoenix", "AZ", "85004"),
            "Bright, casual taqueria with handmade tortillas and salsas.");

        yield return Create(RestaurantIds[9], "The Grill House", "American",
            new Address("700 Oak Street", "Chicago", "IL", "60601"),
            "Dry-aged steaks and craft cocktails in a modern steakhouse setting.");

        yield return Create(RestaurantIds[10], "Liberty Diner", "American",
            new Address("12 Freedom Avenue", "Boston", "MA", "02108"),
            "All-day comfort classics from burgers to bottomless brunch.");

        yield return Create(RestaurantIds[11], "Smokestack BBQ", "American",
            new Address("48 Brisket Lane", "Kansas City", "MO", "64106"),
            "Low-and-slow smoked meats with house-made sauces.");

        yield return Create(RestaurantIds[12], "Maharaja Palace", "Indian",
            new Address("321 Saffron Street", "New York", "NY", "10016"),
            "North Indian curries and tandoori specialties in an opulent hall.");

        yield return Create(RestaurantIds[13], "Spice Route", "Indian",
            new Address("78 Cardamom Court", "Jersey City", "NJ", "07302"),
            "Regional Indian street food and a vibrant chaat counter.");

        yield return Create(RestaurantIds[14], "Tandoori Nights", "Indian",
            new Address("205 Clay Oven Road", "Houston", "TX", "77002"),
            "Charcoal-fired tandoor dishes and freshly baked naan.");

        yield return Create(RestaurantIds[15], "Bombay Bistro", "Indian",
            new Address("63 Monsoon Lane", "Atlanta", "GA", "30303"),
            "Modern Indian small plates with a coastal Mumbai influence.");
    }

    private static Restaurant Create(
        Guid id,
        string name,
        string cuisine,
        Address address,
        string description) => new()
        {
            Id = id,
            Name = name,
            Cuisine = cuisine,
            Address = address,
            Description = description,
            ThumbnailUrl = BuildThumbnailUrl(name),
        };

    // Produces a placeholder image URL using the public placehold.co service. Spaces are
    // replaced with '+' so the restaurant name renders as the placeholder caption.
    private static string BuildThumbnailUrl(string name)
        => $"https://placehold.co/400x300?text={name.Replace(" ", "+")}";
}
