# Task 01: Review Entity & EF Config

## Status

pending

## Wave

1

## Description

Creates the `Review` domain entity, adds it to `RestaurantsDbContext`, creates a Fluent API configuration, and generates an EF migration for the `Reviews` table.

## Dependencies

**Depends on:** STORY-003 task-03-migrations.md
**Blocks:** task-02-review-commands.md

**Context from dependencies:** `RestaurantsDbContext` is in `Data/CM.TableNow.Restaurants.Data`. Migration project is at `Migrations/CM.TableNow.Restaurants.Migrations`.

## Files to Create

- `server/src/Domain/CM.TableNow.Restaurants.Domain/Review.cs`
- `server/src/Data/CM.TableNow.Restaurants.Data/Configurations/ReviewConfiguration.cs`

## Files to Modify

- `server/src/Data/CM.TableNow.Restaurants.Data/RestaurantsDbContext.cs` — Add `DbSet<Review>`

## Technical Details

### Code Snippets

```csharp
// Review.cs
namespace CM.TableNow.Restaurants.Domain;

public class Review
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }     // 1-5
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
}
```

```csharp
// ReviewConfiguration.cs
public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Body).HasMaxLength(2000).IsRequired();
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.HasOne(r => r.Restaurant).WithMany()
               .HasForeignKey(r => r.RestaurantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(r => r.RestaurantId);
    }
}
```

Generate migration:
```powershell
dotnet ef migrations add AddReviews `
  --project src/Migrations/CM.TableNow.Restaurants.Migrations `
  --startup-project src/Api `
  --context RestaurantsDbContext
```

## Acceptance Criteria

- [ ] `Review` entity exists with all fields
- [ ] `RestaurantsDbContext.Reviews` DbSet added
- [ ] EF migration creates `Reviews` table
- [ ] `dotnet build` exits with code 0
