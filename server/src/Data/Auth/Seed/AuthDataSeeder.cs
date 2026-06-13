using CM.TableNow.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Auth.Data.Seed;

/// <summary>
/// Seeds known test user accounts so the MVP can be demonstrated without manual setup.
/// Idempotent: skips seeding entirely if any user already exists.
/// </summary>
public static class AuthDataSeeder
{
    // Work factor 12 is the project minimum for BCrypt hashes. Higher values cost
    // ~exponentially more CPU; 12 balances startup time against brute-force resistance.
    private const int WorkFactor = 12;

    // Shared password for all demo accounts so testers can sign in immediately.
    private const string DemoPassword = "Password123!";

    public static async Task SeedAsync(AuthDbContext db, CancellationToken ct = default)
    {
        // Idempotency guard: if any user exists the seed has already run, so do nothing.
        if (await db.Users.AnyAsync(ct))
        {
            return;
        }

        var createdAt = DateTimeOffset.UtcNow;

        db.Users.AddRange(
            new User
            {
                Id = Guid.Parse("11111111-0000-0000-0000-000000000001"),
                Name = "Alice Demo",
                Email = "alice@tablenow.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(DemoPassword, WorkFactor),
                Role = "Diner",
                CreatedAt = createdAt,
            },
            new User
            {
                Id = Guid.Parse("11111111-0000-0000-0000-000000000002"),
                Name = "Bob Demo",
                Email = "bob@tablenow.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(DemoPassword, WorkFactor),
                Role = "Diner",
                CreatedAt = createdAt,
            });

        await db.SaveChangesAsync(ct);
    }
}
