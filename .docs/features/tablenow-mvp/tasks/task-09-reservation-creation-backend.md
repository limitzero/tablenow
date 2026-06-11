# Task 09: Reservation Creation Backend

## Status

pending

## Phase

5

## Description

Implement `POST /api/reservations` — the most technically critical endpoint in the MVP. When a diner submits a `slotId` and `partySize`, the system must atomically decrement `TimeSlot.RemainingCapacity` and create the `Reservation` record in a single transaction. EF Core optimistic concurrency on `TimeSlot.RowVersion` ensures that two simultaneous requests for the same last seat cannot both succeed. Returns 201 on success, 409 if the slot is full or has been taken, 401 if unauthenticated.

## Dependencies

**Depends on:** task-06-user-signin-jwt-backend, task-07-restaurant-slot-api  
**Blocks:** task-11-reservation-management-backend, task-12-concurrency-integration-test

**Context from dependencies:** task-06 configured JWT bearer authentication and `UseAuthorization()` middleware; reservation endpoints can use `.RequireAuthorization()`. task-07 established the `TimeSlotModel` (with `RowVersion` concurrency token), `RestaurantModel`, and the Mediator/CQRS pattern for the data layer. `AppDbContext` has `DbSet<ReservationModel>` and `DbSet<TimeSlotModel>`. The `userId` claim in the JWT uses the key `"userId"`.

## Files to Create

- `server/src/Application/Reservations/Features/CreateReservation/CreateReservationRequest.cs`
- `server/src/Application/Reservations/Features/CreateReservation/CreateReservationResponse.cs`
- `server/src/Application/Reservations/Features/CreateReservation/CreateReservationRequestHandler.cs`
- `server/src/Data/Reservations/Commands/CreateReservation/CreateReservationCommand.cs`
- `server/src/Data/Reservations/Commands/CreateReservation/CreateReservationCommandHandler.cs`
- `server/src/Api/Reservations/ReservationsEndpoints.cs`
- `server/src/Api/Reservations/ReservationsMapper.cs`
- `server/src/Contracts/Reservations/CreateReservationRequest.cs` (API DTO)
- `server/src/Contracts/Reservations/ReservationDto.cs`
- `server/src/Application/Reservations/ReservationsModule.cs`

## Files to Modify

- `server/src/Api/ServiceCollectionExtensions.cs` — call `services.AddReservationsModule()`
- `server/src/Api/Program.cs` — register `MapReservationEndpoints()` on the API group

## Technical Details

### Implementation Steps

1. **Write Contracts DTOs** for the create reservation request and the response.

2. **Write the Data command handler** — this is where the atomic transaction + optimistic concurrency lives.

3. **Write the Application handler** — validates input, extracts `userId` from context (passed in via request), dispatches the data command.

4. **Write `ReservationsEndpoints`** with `POST /reservations` (requires auth).

5. **Register module and wire endpoints.**

### Code Snippets

**Contracts DTOs:**
```csharp
// src/Contracts/Reservations/CreateReservationRequest.cs
namespace TableNow.Contracts.Reservations;
public sealed record CreateReservationRequest(Guid SlotId, int PartySize);

// src/Contracts/Reservations/ReservationDto.cs
public sealed record ReservationDto(
    Guid ReservationId,
    string RestaurantName,
    string Date,
    string Time,
    int PartySize,
    string Status);
```

**Application request (includes `UserId` extracted from JWT at endpoint):**
```csharp
namespace TableNow.Reservations.Application.Features.CreateReservation;

public sealed record CreateReservationRequest(Guid UserId, Guid SlotId, int PartySize)
    : IRequest<Result<ReservationResponse>>;

public sealed record ReservationResponse(
    Guid ReservationId, string RestaurantName,
    string Date, string Time, int PartySize, string Status);
```

**Application handler:**
```csharp
public sealed class CreateReservationRequestHandler(IMediator mediator)
    : IRequestHandler<CreateReservationRequest, Result<ReservationResponse>>
{
    public async ValueTask<Result<ReservationResponse>> Handle(
        CreateReservationRequest request, CancellationToken cancellationToken)
    {
        if (request.PartySize < 1 || request.PartySize > 20)
            return Result<ReservationResponse>.Failure(400, "Party size must be 1–20.");

        var result = await mediator.Send(
            new CreateReservationCommand(request.UserId, request.SlotId, request.PartySize),
            cancellationToken);

        return result.IsConflict
            ? Result<ReservationResponse>.Failure(409, "This time slot is no longer available.")
            : Result<ReservationResponse>.Success(new ReservationResponse(
                result.ReservationId!, result.RestaurantName!, result.Date!, result.Time!,
                request.PartySize, "Confirmed"), 201);
    }
}
```

**Data command — the concurrency-safe handler:**
```csharp
namespace TableNow.Reservations.Data.Commands.CreateReservation;

public sealed record CreateReservationCommand(Guid UserId, Guid SlotId, int PartySize)
    : IRequest<CreateReservationCommandResult>;

public sealed record CreateReservationCommandResult(
    Guid? ReservationId, string? RestaurantName, string? Date, string? Time, bool IsConflict);

public sealed class CreateReservationCommandHandler(AppDbContext db)
    : IRequestHandler<CreateReservationCommand, CreateReservationCommandResult>
{
    public async ValueTask<CreateReservationCommandResult> Handle(
        CreateReservationCommand command, CancellationToken cancellationToken)
    {
        // Load slot WITH tracking so EF can detect concurrency token changes
        var slot = await db.TimeSlots
            .Include(s => s.Restaurant)
            .FirstOrDefaultAsync(s => s.Id == command.SlotId, cancellationToken);

        if (slot is null || slot.RemainingCapacity < command.PartySize)
            return new CreateReservationCommandResult(null, null, null, null, IsConflict: true);

        slot.RemainingCapacity -= command.PartySize;

        var reservation = new ReservationModel
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            TimeSlotId = command.SlotId,
            PartySize = command.PartySize,
            Status = "Confirmed",
            CreatedAt = DateTime.UtcNow,
        };
        db.Reservations.Add(reservation);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another request modified the slot between our read and write.
            // Re-read and check capacity.
            await db.Entry(slot).ReloadAsync(cancellationToken);
            if (slot.RemainingCapacity < command.PartySize)
                return new CreateReservationCommandResult(null, null, null, null, IsConflict: true);

            // Capacity was restored (e.g., a cancellation occurred) — retry once
            slot.RemainingCapacity -= command.PartySize;
            db.Reservations.Add(reservation with { Id = Guid.NewGuid() });
            await db.SaveChangesAsync(cancellationToken);
        }

        return new CreateReservationCommandResult(
            reservation.Id,
            slot.Restaurant.Name,
            slot.Date.ToString("yyyy-MM-dd"),
            slot.Time.ToString("HH:mm"),
            IsConflict: false);
    }
}
```

**`ReservationsEndpoints.cs`:**
```csharp
namespace TableNow.Api.Reservations;

public static class ReservationsEndpoints
{
    public static RouteGroupBuilder MapReservationEndpoints(this RouteGroupBuilder group)
    {
        var reservations = group.MapGroup("/reservations").RequireAuthorization();

        reservations.MapPost("/", async (
            CreateReservationRequest request,
            IMediator mediator,
            HttpContext context,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(context.User.FindFirstValue("userId")!);
            var result = await mediator.Send(
                ReservationsMapper.ToCreateRequest(request, userId), ct);
            return TypedResultHelper.ToResult(result);
        });

        // GET /reservations/my and DELETE /reservations/{id} added in task-11
        return group;
    }
}
```

**`ReservationsMapper.cs`:**
```csharp
namespace TableNow.Api.Reservations;

public static class ReservationsMapper
{
    public static CreateReservationRequest ToCreateRequest(
        Contracts.Reservations.CreateReservationRequest r, Guid userId) =>
        new(userId, r.SlotId, r.PartySize);
}
```

**API response shapes:**
```json
// POST /api/v1/reservations — 201
{
  "reservationId": "uuid",
  "restaurantName": "Bella Notte",
  "date": "2026-06-20",
  "time": "19:30",
  "partySize": 4,
  "status": "Confirmed"
}

// 409
{ "errors": ["This time slot is no longer available."] }

// 401 (no JWT)
// 400 (invalid partySize)
```

## Acceptance Criteria

- [ ] `POST /api/v1/reservations` with valid JWT + `slotId` + `partySize` returns 201 with reservation details
- [ ] `RemainingCapacity` on the slot is decremented by `partySize` after a successful booking
- [ ] A slot with `RemainingCapacity < partySize` returns 409 with "This time slot is no longer available."
- [ ] Unauthenticated request returns 401
- [ ] The decrement and reservation insert happen in the same `SaveChangesAsync` call (atomic)
- [ ] `DbUpdateConcurrencyException` is caught and handled — double-booking is prevented

## Notes

- **Do NOT use `AsNoTracking()`** when loading the slot for booking — EF needs to track the entity to detect the concurrency conflict via `RowVersion`.
- The `[Timestamp]` concurrency token on `TimeSlotModel.RowVersion` means EF Core automatically includes a `WHERE RowVersion = @original` clause in the `UPDATE`. If the version has changed (another transaction ran), EF throws `DbUpdateConcurrencyException`.
- The retry-once approach after `DbUpdateConcurrencyException` handles the case where a cancellation happened concurrently (freeing capacity). Limit to one retry to avoid an infinite loop.
- `userId` is extracted from the JWT claims at the endpoint level, not from the request body. `context.User.FindFirstValue("userId")` reads the custom claim set in task-06.
