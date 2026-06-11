# Task 02: Wire Email into Reservation Handler

## Status

pending

## Wave

2

## Description

Updates `CreateReservationRequestHandler` to fire a confirmation email via `IEmailService` after a successful booking. The email call is fire-and-forget (wrapped in `Task.Run` or `_ = emailService.SendAsync(...)`). The reservation response is returned immediately regardless of email status.

## Dependencies

**Depends on:** task-01-email-template.md, STORY-014 task-03-app-handler.md
**Blocks:** Nothing

**Context from dependencies:**
- task-01 created `ConfirmationEmailBuilder.Build(toEmail, toName, restaurantName, restaurantAddress, slotDateTime, partySize)` → `EmailMessage`
- STORY-014 task-03 created `CreateReservationRequestHandler` in `Application/Reservations/Features/CreateReservation/`
- `IEmailService` is registered in DI (STORY-020)
- The handler needs user email/name (from a `GetUserByIdQuery`) and restaurant name/address (from a `GetRestaurantByIdQuery`) to build the email

## Files to Modify

- `server/src/Application/CM.TableNow.Reservations.Application/Features/CreateReservation/CreateReservationRequestHandler.cs` — Add IEmailService injection and fire-and-forget email call

## Technical Details

### Code Snippets

Add to `CreateReservationRequestHandler`:
```csharp
// Add IEmailService injection and after successful booking:
_ = Task.Run(async () =>
{
    try
    {
        // Fetch user details for email
        var user = await mediator.Send(new GetUserByIdQuery(request.UserId));
        var restaurant = await mediator.Send(new GetRestaurantByIdQuery(slotRestaurantId));
        if (user is not null && restaurant is not null)
        {
            var message = ConfirmationEmailBuilder.Build(
                user.Email, user.Name,
                restaurant.Name, restaurant.Address,
                slotDateTime, request.PartySize);
            await emailService.SendAsync(message);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to send confirmation email for reservation {Id}", reservationId);
    }
}, CancellationToken.None);
```

Note: `GetUserByIdQuery` needs to be created (add to STORY-005's data queries folder if not already present) and `slotDateTime` needs to be returned from `CreateReservationCommand` result.

## Acceptance Criteria

- [ ] Email is sent after successful booking (verifiable via email provider dashboard)
- [ ] Booking response (201) is returned regardless of email outcome
- [ ] Email failure is caught and logged, not propagated
- [ ] `dotnet build` exits with code 0
