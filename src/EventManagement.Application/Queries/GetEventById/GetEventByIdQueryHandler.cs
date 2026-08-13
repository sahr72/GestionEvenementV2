// EventManagement.Application/Queries/GetEventByIdQueryHandler.cs

using EventManagement.Application.Interfaces;
using EventManagement.Application.ReadModels;

namespace EventManagement.Application.Queries;

public sealed class GetEventByIdQueryHandler
{
    private readonly IEvenementQueryService _eventQueryService;

    public GetEventByIdQueryHandler(IEvenementQueryService eventQueryService)
    {
        _eventQueryService = eventQueryService;
    }

    public Task<EventDetailsReadModel?> Handle(
        GetEventByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        return _eventQueryService.GetEventByIdAsync(query.Id, cancellationToken);
    }
}