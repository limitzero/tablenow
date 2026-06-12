namespace CM.TableNow.Contracts;

public sealed record TimeSlotResponse(
    Guid SlotId,
    DateTimeOffset Time,
    int RemainingCapacity);
