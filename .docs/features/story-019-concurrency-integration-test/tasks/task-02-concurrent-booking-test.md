# Task 02: Concurrent Booking Test

## Status

pending

## Wave

2

## Description

Implements the actual concurrent booking test using the `api_fixture`. Seeds a slot with `RemainingCapacity = 1`, fires two simultaneous booking requests, and asserts exactly one succeeds.

## Dependencies

**Depends on:** task-01-integration-test-infra.md
**Blocks:** Nothing

**Context from dependencies:** task-01 created `api_fixture` with `AuthenticateAsync()` and `SeedSlotAsync(capacity)`. This test class uses the fixture as a constructor parameter (xUnit `IClassFixture<api_fixture>` pattern).

## Files to Create

- `server/tests/IntegrationTests/Reservations/describe_concurrent_booking/when_two_users_book_the_last_slot.cs`

## Technical Details

### Code Snippets

```csharp
// describe_concurrent_booking/when_two_users_book_the_last_slot.cs
namespace describe_concurrent_booking;

public class when_two_users_book_the_last_slot(api_fixture fixture)
    : IClassFixture<api_fixture>
{
    [Fact]
    public async Task it_should_allow_only_one_booking()
    {
        // Arrange: slot with exactly 1 remaining seat
        var slotId = await fixture.SeedSlotAsync(remainingCapacity: 1);
        var token = await fixture.AuthenticateAsync();

        var client1 = fixture.CreateClient();
        var client2 = fixture.CreateClient();
        client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var body = JsonContent.Create(new { slotId, partySize = 1 });

        // Act: fire both requests at the same time
        var (res1, res2) = await (
            client1.PostAsync("/api/reservations", body),
            client2.PostAsync("/api/reservations", body)
        ).WhenBoth();

        // A simpler Task.WhenAll approach:
        // var results = await Task.WhenAll(
        //     client1.PostAsJsonAsync("/api/reservations", new { slotId, partySize = 1 }),
        //     client2.PostAsJsonAsync("/api/reservations", new { slotId, partySize = 1 })
        // );

        var statusCodes = new[] { res1.StatusCode, res2.StatusCode };

        // Assert: exactly one 201 and one 409
        statusCodes.Should().Contain(HttpStatusCode.Created, "one booking must succeed");
        statusCodes.Should().Contain(HttpStatusCode.Conflict, "the other must be rejected");
        statusCodes.Should().HaveCount(2);
    }
}
```

**Simpler Task.WhenAll version (preferred):**

```csharp
[Fact]
public async Task it_should_allow_only_one_booking()
{
    var slotId = await fixture.SeedSlotAsync(remainingCapacity: 1);
    var token = await fixture.AuthenticateAsync();

    var results = await Task.WhenAll(
        PostBookingAsync(slotId, 1, token),
        PostBookingAsync(slotId, 1, token)
    );

    var codes = results.Select(r => r.StatusCode).ToList();
    codes.Should().Contain(HttpStatusCode.Created);
    codes.Should().Contain(HttpStatusCode.Conflict);
}

private async Task<HttpResponseMessage> PostBookingAsync(Guid slotId, int partySize, string token)
{
    var client = fixture.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    return await client.PostAsJsonAsync("/api/reservations", new { slotId, partySize });
}
```

## Acceptance Criteria

- [ ] Test class uses `api_fixture` via `IClassFixture<api_fixture>`
- [ ] `Task.WhenAll` fires both requests concurrently
- [ ] Asserts exactly one `HttpStatusCode.Created` (201)
- [ ] Asserts exactly one `HttpStatusCode.Conflict` (409)
- [ ] `dotnet test --filter "FullyQualifiedName~describe_concurrent_booking"` passes
