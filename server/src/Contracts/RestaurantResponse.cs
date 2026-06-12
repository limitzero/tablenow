namespace CM.TableNow.Contracts;

public sealed record RestaurantResponse(
    Guid Id,
    string Name,
    string Cuisine,
    string Address,
    string Description,
    string ThumbnailUrl);
