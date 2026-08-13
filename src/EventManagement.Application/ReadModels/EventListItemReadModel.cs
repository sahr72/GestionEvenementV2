namespace EventManagement.Application.ReadModels;

public sealed record EventListItemReadModel
{
    public required Guid Id { get; init; }
    public required string Titre { get; init; }
    public required string PersonneImpliquee { get; init; }
}