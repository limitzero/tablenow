# ADR-001: EF Core RowVersion as Optimistic Concurrency Token

## Status

Accepted

## Date

2026-06-13

## Context

TableNow must prevent double-booking: two concurrent requests may both read the same `TimeSlot` with available capacity and both attempt to decrement `RemainingCapacity`. Without a concurrency guard the second write silently overwrites the first, resulting in over-booked slots.

EF Core supports optimistic concurrency via a concurrency token — a property that EF includes in the `WHERE` clause of every `UPDATE`. If the value has changed since the entity was loaded, EF throws `DbUpdateConcurrencyException` instead of silently overwriting.

Two options were considered:

| Option | Behaviour |
|---|---|
| `.IsConcurrencyToken()` only | Works on any column type; EF checks the value but does not auto-generate it — the application must update it manually on every write. |
| `.IsRowVersion()` | Marks the property as a database-generated, auto-incrementing byte array. Implies `IsConcurrencyToken()`. Requires a `byte[]` property named by convention (`RowVersion`). |

The project targets SQLite (dev) and SQL Server (prod). EF Core maps `IsRowVersion()` to a `BLOB` managed by a trigger on SQLite and to a native `rowversion` column on SQL Server — both produce the correct concurrency semantics without any application-level bookkeeping.

## Decision

For every entity that requires optimistic concurrency protection, add a `byte[] RowVersion` property to the domain entity and configure it in the entity's `IEntityTypeConfiguration<T>` using `IsRowVersion()`:

```csharp
// In IEntityTypeConfiguration<TEntity>.Configure():
builder.Property(x => x.RowVersion)
    .IsRowVersion();
```

`IsRowVersion()` already implies `IsConcurrencyToken()` — do not add both.

The domain entity carries the property as a plain `byte[]`; no EF annotations are placed on the domain model itself:

```csharp
// Domain entity (no EF annotations)
public class TimeSlot
{
    public Guid Id { get; set; }
    // ...
    public byte[] RowVersion { get; set; } = [];
}
```

The Fluent API configuration is the single source of truth for the concurrency behaviour.

## Consequences

- EF Core automatically includes the `RowVersion` value in `UPDATE WHERE` clauses; handlers do not need to increment the token manually.
- On concurrent writes, the losing writer receives a `DbUpdateConcurrencyException`. Reservation handlers must catch this and return an appropriate `Result` (e.g., `409 Conflict`) rather than letting the exception propagate.
- SQLite requires a migration-generated trigger to auto-increment the BLOB; this is handled by `dotnet ef migrations add` when targeting SQLite.
- SQL Server maps the column to `rowversion NOT NULL` natively — no trigger is needed.
- Entities that do not require concurrency protection (e.g., `User`, `Restaurant`) do not need a `RowVersion` property.

## Reference

- `server/src/Data/Restaurants/Configurations/TimeSlotConfiguration.cs` — canonical example
- EF Core docs: [Optimistic Concurrency](https://learn.microsoft.com/en-us/ef/core/saving/concurrency)
