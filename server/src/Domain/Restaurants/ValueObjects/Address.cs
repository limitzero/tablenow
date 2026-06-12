namespace CM.TableNow.Restaurants.Domain.ValueObjects;

public sealed record Address(
    string Street,
    string City,
    string Region,
    string PostalCode);
