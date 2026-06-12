namespace CM.TableNow.Auth.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string Role { get; set; } = "Diner";
    public DateTimeOffset CreatedAt { get; set; }
}
