using CM.TableNow.Reservations.Domain.Enums;

namespace CM.TableNow.Reservations.Domain.Entities;

public sealed class Reservation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TimeSlotId { get; set; }
    public int PartySize { get; set; }
    public ReservationStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
