# Task 02: EF Core Models & Fluent API Configurations

## Status

pending

## Wave

1

## Description

Creates one DbContext per business context and Fluent API `IEntityTypeConfiguration<T>` classes for each entity. The domain entities from task-01 are reused directly as EF models (no separate persistence model). `OnModelCreating` calls `ApplyConfigurationsFromAssembly` so new configurations are picked up automatically.

## Dependencies

**Depends on:** task-01-solution-projects.md (from STORY-001 — EF NuGet packages must be installed)
**Blocks:** task-03-migrations.md

**Context from dependencies:** STORY-001 task-01 installed EF Core packages in the Data projects and added project references from Data → Domain. task-01 of this story (running in parallel) creates the domain entity classes — the entity shapes are described in that task file's code snippets, so implementation can proceed concurrently.

The entity shapes are:
- `User`: Id (Guid), Name, Email, PasswordHash, Role, CreatedAt
- `Restaurant`: Id, Name, Cuisine, Address, Description, ThumbnailUrl?, CreatedAt, TimeSlots nav
- `TimeSlot`: Id, RestaurantId, SlotDateTime, TotalCapacity, RemainingCapacity, RowVersion (byte[]), Restaurant nav
- `Reservation`: Id, UserId, TimeSlotId, PartySize, Status, CreatedAt

## Files to Create

- `server/src/Data/CM.TableNow.Auth.Data/AuthDbContext.cs`
- `server/src/Data/CM.TableNow.Auth.Data/Configurations/UserConfiguration.cs`
- `server/src/Data/CM.TableNow.Restaurants.Data/RestaurantsDbContext.cs`
- `server/src/Data/CM.TableNow.Restaurants.Data/Configurations/RestaurantConfiguration.cs`
- `server/src/Data/CM.TableNow.Restaurants.Data/Configurations/TimeSlotConfiguration.cs`
- `server/src/Data/CM.TableNow.Reservations.Data/ReservationsDbContext.cs`
- `server/src/Data/CM.TableNow.Reservations.Data/Configurations/ReservationConfiguration.cs`

## Files to Modify

None.

## Technical Details

### Implementation Steps

1. Create each DbContext referencing the domain entities.
2. Create an `IEntityTypeConfiguration<T>` class for each entity in a `Configurations/` subfolder.
3. In each `OnModelCreating`, call `ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly())`.

### Code Snippets

```csharp
// AuthDbContext.cs
using CM.TableNow.Auth.Domain;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Auth.Data;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}
```

```csharp
// Configurations/UserConfiguration.cs
using CM.TableNow.Auth.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CM.TableNow.Auth.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Name).HasMaxLength(200).IsRequired();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Role).HasMaxLength(50).HasDefaultValue("Diner");
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
```

```csharp
// RestaurantsDbContext.cs
using CM.TableNow.Restaurants.Domain;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Restaurants.Data;

public class RestaurantsDbContext(DbContextOptions<RestaurantsDbContext> options) : DbContext(options)
{
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RestaurantsDbContext).Assembly);
    }
}
```

```csharp
// Configurations/TimeSlotConfiguration.cs
using CM.TableNow.Restaurants.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CM.TableNow.Restaurants.Data.Configurations;

public class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
{
    public void Configure(EntityTypeBuilder<TimeSlot> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.RemainingCapacity).IsRequired();
        builder.Property(t => t.TotalCapacity).IsRequired();
        builder.Property(t => t.SlotDateTime).IsRequired();

        // Optimistic concurrency token — prevents double-booking
        builder.Property(t => t.RowVersion).IsRowVersion();

        builder.HasOne(t => t.Restaurant)
               .WithMany(r => r.TimeSlots)
               .HasForeignKey(t => t.RestaurantId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
```

```csharp
// ReservationsDbContext.cs
using CM.TableNow.Reservations.Domain;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Reservations.Data;

public class ReservationsDbContext(DbContextOptions<ReservationsDbContext> options) : DbContext(options)
{
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReservationsDbContext).Assembly);
    }
}
```

```csharp
// Configurations/ReservationConfiguration.cs
using CM.TableNow.Reservations.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CM.TableNow.Reservations.Data.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Status).HasMaxLength(50).HasDefaultValue("Confirmed");
        builder.Property(r => r.PartySize).IsRequired();
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Index for efficient "my reservations" queries
        builder.HasIndex(r => r.UserId);
    }
}
```

## Acceptance Criteria

- [ ] Three DbContext classes exist, one per business context
- [ ] Each `OnModelCreating` calls `ApplyConfigurationsFromAssembly`
- [ ] `TimeSlotConfiguration` marks `RowVersion` with `.IsRowVersion()`
- [ ] `UserConfiguration` has a unique index on `Email`
- [ ] `ReservationConfiguration` has an index on `UserId`
- [ ] No EF attributes appear on domain entity classes (all mapping is in configuration classes)
- [ ] `dotnet build` exits with code 0

## Notes

The three DbContexts are intentionally separate — Auth, Restaurants, and Reservations are independent business contexts. Cross-context joins (e.g., a reservation joined to a restaurant name) must be done at the Application layer by dispatching two queries through Mediator, not via EF navigation properties across DbContexts.
