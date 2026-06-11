# Task 01: Create Domain Entities

## Status

pending

## Wave

1

## Description

Creates plain C# domain entity classes for all four core tables: `User`, `Restaurant`, `TimeSlot`, and `Reservation`. These classes have no EF Core annotations — they are pure domain objects. They live in `Domain/<Context>/` projects and are referenced by both Application and Data layers.

## Dependencies

**Depends on:** task-01-solution-projects.md (from STORY-001 — the .csproj files must exist)
**Blocks:** task-03-migrations.md

**Context from dependencies:** STORY-001 task-01 created the solution with `CM.TableNow.Auth.Domain`, `CM.TableNow.Restaurants.Domain`, and `CM.TableNow.Reservations.Domain` class library projects. This task populates those projects with entity classes.

## Files to Create

- `server/src/Domain/CM.TableNow.Auth.Domain/User.cs`
- `server/src/Domain/CM.TableNow.Restaurants.Domain/Restaurant.cs`
- `server/src/Domain/CM.TableNow.Restaurants.Domain/TimeSlot.cs`
- `server/src/Domain/CM.TableNow.Reservations.Domain/Reservation.cs`

## Files to Modify

None.

## Technical Details

### Implementation Steps

1. Create `User.cs` in the Auth domain project.
2. Create `Restaurant.cs` and `TimeSlot.cs` in the Restaurants domain project.
3. Create `Reservation.cs` in the Reservations domain project.
4. Keep these as plain classes/records — zero EF attributes.

### Code Snippets

```csharp
// server/src/Domain/CM.TableNow.Auth.Domain/User.cs
namespace CM.TableNow.Auth.Domain;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Diner"; // "Diner" | "Operator"
    public DateTime CreatedAt { get; set; }
}
```

```csharp
// server/src/Domain/CM.TableNow.Restaurants.Domain/Restaurant.cs
namespace CM.TableNow.Restaurants.Domain;

public class Restaurant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Cuisine { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<TimeSlot> TimeSlots { get; set; } = [];
}
```

```csharp
// server/src/Domain/CM.TableNow.Restaurants.Domain/TimeSlot.cs
namespace CM.TableNow.Restaurants.Domain;

public class TimeSlot
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public DateTime SlotDateTime { get; set; }
    public int TotalCapacity { get; set; }
    public int RemainingCapacity { get; set; }

    // Concurrency token — set by EF via Fluent API in task-02
    public byte[] RowVersion { get; set; } = [];

    public Restaurant Restaurant { get; set; } = null!;
}
```

```csharp
// server/src/Domain/CM.TableNow.Reservations.Domain/Reservation.cs
namespace CM.TableNow.Reservations.Domain;

public class Reservation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TimeSlotId { get; set; }
    public int PartySize { get; set; }
    public string Status { get; set; } = "Confirmed"; // "Confirmed" | "Cancelled"
    public DateTime CreatedAt { get; set; }
}
```

## Acceptance Criteria

- [ ] `User`, `Restaurant`, `TimeSlot`, `Reservation` classes exist at the specified paths
- [ ] Zero EF Core attributes (no `[Key]`, `[Column]`, `[Table]`, etc.) on any domain entity
- [ ] `TimeSlot` has a `RowVersion` byte array property for optimistic concurrency
- [ ] `Restaurant` has a navigation property `ICollection<TimeSlot> TimeSlots`
- [ ] `dotnet build` exits with code 0
- [ ] All files use file-scoped namespaces and nullable enabled

## Notes

`RowVersion` is a `byte[]` property. The EF Fluent API configuration (task-02) marks it as a concurrency token using `.IsRowVersion()`. The domain entity itself needs no annotation — the Fluent config does all the work.
