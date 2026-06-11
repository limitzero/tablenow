# Task 01: Domain Entities

## Status

pending

## Wave

1

## Description

Creates the four core domain entity classes — `User`, `Restaurant`, `TimeSlot`, and `Reservation` — in their respective Domain projects. These are pure C# classes with no EF Core attributes. The separation keeps the domain model clean of persistence concerns. All EF configuration is deferred to task-02.

## Dependencies

**Depends on:** None (Wave 1 — but requires STORY-001 to have completed so project files exist)
**Blocks:** task-02-ef-models-configs.md

**Context from dependencies:** STORY-001 created `server/src/Domain/Auth/Domain.Auth.csproj`, `server/src/Domain/Restaurants/Domain.Restaurants.csproj`, and `server/src/Domain/Reservations/Domain.Reservations.csproj`. This task creates entity classes inside those projects.

## Files to Create

- `server/src/Domain/Auth/User.cs`
- `server/src/Domain/Restaurants/Restaurant.cs`
- `server/src/Domain/Restaurants/TimeSlot.cs`
- `server/src/Domain/Reservations/Reservation.cs`

## Technical Details

### Implementation Steps

1. Create each entity as a plain C# class — no `[Key]`, `[Required]`, `[Column]`, or any System.ComponentModel.DataAnnotations attributes.

2. Use file-scoped namespaces and C# primary constructors where appropriate.

### Code Snippets

```csharp
// server/src/Domain/Auth/User.cs
namespace TableNow.Domain.Auth;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Diner"; // "Diner" | "Operator"
    public DateTimeOffset CreatedAt { get; set; }
}
```

```csharp
// server/src/Domain/Restaurants/Restaurant.cs
namespace TableNow.Domain.Restaurants;

public class Restaurant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Cuisine { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<TimeSlot> TimeSlots { get; set; } = [];
}
```

```csharp
// server/src/Domain/Restaurants/TimeSlot.cs
namespace TableNow.Domain.Restaurants;

public class TimeSlot
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public DateTimeOffset DateTime { get; set; }
    public int TotalCapacity { get; set; }
    public int RemainingCapacity { get; set; }
    public byte[] RowVersion { get; set; } = []; // optimistic concurrency token
    public Restaurant Restaurant { get; set; } = null!;
}
```

```csharp
// server/src/Domain/Reservations/Reservation.cs
namespace TableNow.Domain.Reservations;

public class Reservation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SlotId { get; set; }
    public int PartySize { get; set; }
    public string Status { get; set; } = "Confirmed"; // "Confirmed" | "Cancelled"
    public DateTimeOffset CreatedAt { get; set; }
    public bool ReminderSent { get; set; } // set in STORY-022
}
```

## Acceptance Criteria

- [ ] All four entity classes exist in their correct Domain projects
- [ ] No EF attributes (`[Key]`, `[Required]`, `[Column]`, etc.) on any entity
- [ ] `TimeSlot.RowVersion` is `byte[]`
- [ ] `Reservation.Status` is a string with "Confirmed" as default
- [ ] All projects compile after adding these files
