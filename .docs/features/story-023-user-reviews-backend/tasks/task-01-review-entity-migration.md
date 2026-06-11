# Task 01: Review Entity & Migration

## Status

pending

## Wave

1

## Description

Creates the `Review` domain entity in `Domain/Restaurants/`, EF configuration, adds `DbSet<Review>` to `AppDbContext`, and runs a new migration.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-003 AppDbContext)
**Blocks:** task-02-submit-review-handler-endpoint.md, task-03-get-reviews-handler-endpoint.md

**Context from dependencies:** STORY-003 created `AppDbContext` in `Data/Reservations/AppDbContext.cs`. This task adds `DbSet<Review>` to that context and creates the EF configuration in `Data/Restaurants/Configurations/ReviewConfiguration.cs`.

## Files to Create

- `server/src/Domain/Restaurants/Review.cs`
- `server/src/Data/Restaurants/Configurations/ReviewConfiguration.cs`

## Files to Modify

- `server/src/Data/Reservations/AppDbContext.cs` — add `DbSet<Review>`

## Technical Details

### Code Snippets

```csharp
// Domain/Restaurants/Review.cs
namespace TableNow.Domain.Restaurants;
public class Review
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; } // 1-5
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
}
```

```csharp
// Data/Restaurants/Configurations/ReviewConfiguration.cs
public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Body).HasMaxLength(2000).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.HasOne(r => r.Restaurant)
               .WithMany()
               .HasForeignKey(r => r.RestaurantId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
```

After creating files: `dotnet ef migrations add AddReviews --project server/src/Migrations --startup-project server/src/Migrations`

## Acceptance Criteria

- [ ] `Review` entity has Id, RestaurantId, UserId, Rating, Body, CreatedAt
- [ ] `ReviewConfiguration` sets max length on Body, configures FK to Restaurant
- [ ] `AppDbContext.Reviews` DbSet added
- [ ] New migration generated with Reviews table
