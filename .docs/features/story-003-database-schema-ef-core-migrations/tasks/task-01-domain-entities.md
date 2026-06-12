# Task 01: Domain Entities

## Status

pending

## Wave

1

## Description

Define the four core domain entities for TableNow — `User`, `Restaurant`, `TimeSlot`, and `Reservation` — as pure business types in their respective `Domain/<Context>/` projects. These types carry no EF Core annotations (mapping is handled separately by Fluent API in task-02). Value objects are used where they clarify the model (e.g., `Address`, `Email`). This task establishes the shapes that the EF models, migrations, seed data, and every feature handler will reference.

## Dependencies

**Depends on:** None (Wave 1) — relies only on the STORY-001 scaffold existing.
**Blocks:** task-02-ef-models-configs, task-03-migrations-project

**Context from dependencies:** STORY-001 created the per-context `Domain` projects: `CM.TableNow.Auth.Domain`, `CM.TableNow.Restaurants.Domain`, `CM.TableNow.Reservations.Domain`, each referencing only `CM.TableNow.Shared`. This task adds entity types into those projects. task-02 will map these exact shapes to EF models, so keep property names and types stable.

## Files to Create

- `server/src/Domain/Auth/Entities/User.cs` — the `User` entity.
- `server/src/Domain/Auth/ValueObjects/Email.cs` — an `Email` value object (optional but preferred).
- `server/src/Domain/Restaurants/Entities/Restaurant.cs` — the `Restaurant` entity.
- `server/src/Domain/Restaurants/Entities/TimeSlot.cs` — the `TimeSlot` entity (carries `RemainingCapacity` + `RowVersion`).
- `server/src/Domain/Restaurants/ValueObjects/Address.cs` — an `Address` value object.
- `server/src/Domain/Reservations/Entities/Reservation.cs` — the `Reservation` entity.
- `server/src/Domain/Reservations/Enums/ReservationStatus.cs` — the `ReservationStatus` enum.

## Files to Modify

- None (additive to STORY-001 projects).

## Technical Details

### Entity shapes

**User** (Auth context):
- `Guid Id`
- `string Name`
- `Email Email` (value object wrapping a validated email string) — or `string Email` if value object is impractical for EF mapping; prefer the value object with a converter handled in task-02.
- `string PasswordHash` (BCrypt hash, populated by STORY-005)
- `string Role` (e.g., `"Diner"`, `"Operator"`; default `"Diner"`)
- `DateTimeOffset CreatedAt`

**Restaurant** (Restaurants context):
- `Guid Id`
- `string Name`
- `string Cuisine`
- `Address Address` (value object: `Street`, `City`, `Region`, `PostalCode`)
- `string Description`
- `string ThumbnailUrl`
- navigation: `ICollection<TimeSlot> TimeSlots`

**TimeSlot** (Restaurants context):
- `Guid Id`
- `Guid RestaurantId`
- `DateTimeOffset StartTime` (the slot's date+time)
- `int TotalCapacity`
- `int RemainingCapacity` — decremented on booking, restored on cancel
- `byte[] RowVersion` — optimistic-concurrency token (configured in task-02)
- navigation: `Restaurant Restaurant`

**Reservation** (Reservations context):
- `Guid Id`
- `Guid UserId`
- `Guid TimeSlotId`
- `int PartySize`
- `ReservationStatus Status`
- `DateTimeOffset CreatedAt`

**ReservationStatus** enum: `Confirmed`, `Cancelled`.

### Implementation Steps

1. Create the `User` entity in the Auth domain project. Use an `Email` value object (a record wrapping a `string Value` with a basic format guard) if practical; otherwise a `string Email`.
2. Create the `Address` value object (record with `Street`, `City`, `Region`, `PostalCode`).
3. Create the `Restaurant` and `TimeSlot` entities in the Restaurants domain project. `TimeSlot` includes `RemainingCapacity` and a `byte[] RowVersion`.
4. Create the `Reservation` entity and `ReservationStatus` enum in the Reservations domain project.
5. Use file-scoped namespaces, nullable reference types, and `required`/init-only properties or constructors as appropriate. No EF attributes.
6. Build to confirm the domain projects compile.

### Code Snippets

```csharp
namespace CM.TableNow.Auth.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string Role { get; set; } = "Diner";
    public DateTimeOffset CreatedAt { get; set; }
}
```

```csharp
namespace CM.TableNow.Restaurants.Domain.Entities;

public sealed class TimeSlot
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public int TotalCapacity { get; set; }
    public int RemainingCapacity { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Restaurant Restaurant { get; set; } = null!;
}
```

```csharp
namespace CM.TableNow.Reservations.Domain.Enums;

public enum ReservationStatus
{
    Confirmed = 1,
    Cancelled = 2,
}
```

## Acceptance Criteria

- [ ] `User`, `Restaurant`, `TimeSlot`, and `Reservation` entities exist in their respective `Domain/<Context>/` projects.
- [ ] No entity carries any EF Core attribute (no `[Key]`, `[Column]`, `[Timestamp]`, etc.).
- [ ] `TimeSlot` has `RemainingCapacity` (int) and a `RowVersion` (`byte[]`) property.
- [ ] `Reservation.Status` is typed as the `ReservationStatus` enum with `Confirmed` and `Cancelled`.
- [ ] `Address` is modeled as a value object; `Email` is a value object or a plain string (documented choice).
- [ ] The domain projects compile (`dotnet build`).

## Notes

- Keep the domain free of persistence concerns: no DbContext references, no EF packages in the Domain projects.
- The `RowVersion` property's mapping (rowversion vs. concurrency token) is configured in task-02; here it is just a `byte[]` property so the shape exists.
- Do not add a `Reservation` navigation to `User` across contexts — `Reservation` references `UserId` by value only (cross-context references stay loose; joins happen in Data queries by id).
