namespace EventManagement.Application.ReadModels;

public sealed record EventListReadModel
{
    public required IReadOnlyList<EventListItemReadModel> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
}