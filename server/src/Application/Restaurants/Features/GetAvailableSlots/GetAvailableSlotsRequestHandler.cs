using CM.TableNow.Contracts;
using CM.TableNow.Restaurants.Data.Queries.GetAvailableSlots;
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Restaurants.Application.Features.GetAvailableSlots;

public sealed class GetAvailableSlotsRequestHandler(IMediator mediator)
    : IRequestHandler<GetAvailableSlotsRequest, Result<IReadOnlyList<TimeSlotResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<TimeSlotResponse>>> Handle(
        GetAvailableSlotsRequest request,
        CancellationToken cancellationToken)
    {
        var dataResult = await mediator.Send(
            new GetAvailableSlotsQuery(request.RestaurantId, request.Date, request.PartySize),
            cancellationToken);

        if (!dataResult.IsSuccess)
            return Result<IReadOnlyList<TimeSlotResponse>>.Failure(dataResult.StatusCode, [.. dataResult.Errors]);

        IReadOnlyList<TimeSlotResponse> responses = dataResult.Data!
            .Select(d => new TimeSlotResponse(d.SlotId, d.Time, d.RemainingCapacity))
            .ToList();

        return Result<IReadOnlyList<TimeSlotResponse>>.Success(responses);
    }
}
