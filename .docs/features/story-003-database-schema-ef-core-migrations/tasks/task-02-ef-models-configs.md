# Task 02: EF Models & Configurations

## Status

pending

## Wave

2

## Description

Creates the EF Core Fluent API configuration classes for each entity and the `AppDbContext` that wires them together. Configurations live in `Data/<Context>/Configurations/` and are applied via `ApplyConfigurationsFromAssembly`. The critical configuration here is `TimeSlot.RowVersion` as a concurrency token, which is essential for STORY-014's double-booking prevention.

## Dependencies

**Depends on:** task-01-domain-entities.md
**Blocks:** task-03-migrations-project.md

**Context from dependencies:** task-01 created the four domain entity classes (`User`, `Restaurant`, `TimeSlot`, `Reservation`) in Domain projects. These entities are referenced in the EF configurations and the DbContext created here.

## Files to Create

- `server/src/Data/Auth/Configurations/UserConfiguration.cs`
- `server/src/Data/Restaurants/Configurations/RestaurantConfiguration.cs`
- `server/src/Data/Restaurants/Configurations/TimeSlotConfiguration.cs`
- `server/src/Data/Reservations/Configurations/ReservationConfiguration.cs`
- `server/src/Data/Reservations/AppDbContext.cs` — single DbContext for all entities

## Technical Details

### Implementation Steps

1. Each configuration class implements `IEntityTypeConfiguration<TEntity>` and uses Fluent API only.

2. Create a single `AppDbContext` in `Data/Reservations/` (or a shared location) with `DbSet` properties for all entities. Override `OnModelCreating` to call `ApplyConfigurationsFromAssembly` for each Data assembly.

3. Register `AppDbContext` in the module registration (update the stub from STORY-001).

### Code Snippets

```csharp
// server/src/Data/Auth/Configurations/UserConfiguration.cs
namespace TableNow.Data.Auth.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(320).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Role).HasMaxLength(50).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();
    }
}
```

```csharp
// server/src/Data/Restaurants/Configurations/TimeSlotConfiguration.cs
namespace TableNow.Data.Restaurants.Configurations;

public class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
{
    public void Configure(EntityTypeBuilder<TimeSlot> builder)
    {
        builder.HasKey(ts => ts.Id);
        builder.Property(ts => ts.DateTime).IsRequired();
        builder.Property(ts => ts.TotalCapacity).IsRequired();
        builder.Property(ts => ts.RemainingCapacity).IsRequired();
        builder.Property(ts => ts.RowVersion).IsRowVersion(); // optimistic concurrency token
        builder.HasOne(ts => ts.Restaurant)
               .WithMany(r => r.TimeSlots)
               .HasForeignKey(ts => ts.RestaurantId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
```

```csharp
// server/src/Data/Reservations/AppDbContext.cs
namespace TableNow.Data.Reservations;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(TableNow.Data.Auth.Configurations.UserConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(TableNow.Data.Restaurants.Configurations.RestaurantConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ReservationConfiguration).Assembly);
    }
}
```

## Acceptance Criteria

- [ ] Four `IEntityTypeConfiguration<T>` classes exist with correct Fluent API mappings
- [ ] `TimeSlotConfiguration` calls `.IsRowVersion()` on the `RowVersion` property
- [ ] `AppDbContext` has `DbSet` for all four entities
- [ ] `OnModelCreating` uses `ApplyConfigurationsFromAssembly` — no inline `modelBuilder.Entity<>()` calls
- [ ] `AppDbContext` is registered in DI (via `AddDbContext` in module registration)
