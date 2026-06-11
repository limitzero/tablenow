# Task 01: Photo Entity & Migration

## Status

pending

## Wave

1

## Description

Creates the `Photo` domain entity, EF configuration, adds `DbSet<Photo>` to `AppDbContext`, and runs a new EF migration.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-003 AppDbContext)
**Blocks:** task-02-upload-handler-validation.md

**Context from dependencies:** STORY-003 created `AppDbContext` with DbSets. STORY-010 created `RestaurantDto` in Contracts. This task adds a `photos` array to the restaurant detail response by updating `RestaurantDto` or creating a new overloaded response type.

## Files to Create

- `server/src/Domain/Restaurants/Photo.cs`
- `server/src/Data/Restaurants/Configurations/PhotoConfiguration.cs`

## Files to Modify

- `server/src/Data/Reservations/AppDbContext.cs` — add `DbSet<Photo>`
- `server/src/Contracts/Restaurants/RestaurantDto.cs` — add `Photos` array to detail response

## Technical Details

### Code Snippets

```csharp
// Domain/Restaurants/Photo.cs
namespace TableNow.Domain.Restaurants;
public class Photo
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTimeOffset UploadedAt { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
}
```

```csharp
// PhotoConfiguration.cs
public class PhotoConfiguration : IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Url).HasMaxLength(1000).IsRequired();
        builder.HasOne(p => p.Restaurant).WithMany().HasForeignKey(p => p.RestaurantId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
```

After adding files: `dotnet ef migrations add AddPhotos --project server/src/Migrations --startup-project server/src/Migrations`

## Acceptance Criteria

- [ ] `Photo` entity with Id, RestaurantId, Url, UploadedAt
- [ ] `PhotoConfiguration` sets max length on Url and configures FK
- [ ] `AppDbContext.Photos` DbSet added
- [ ] Migration generated
