namespace EventManagement.Application.ReadModels;
/*record avec des propriétés required, mais sans constructeur positionnel.
public sealed record CommentReadModel
{
    public required Guid Id { get; init; }
    public required string Description { get; init; }
    public required DateTime Date { get; init; }
    public required Guid EventId { get; init; }
}
*/


public sealed record CommentReadModel(
    Guid Id,
    Guid EventId,
    string description,
    DateTimeOffset DateCreationUtcUtc,
    DateTimeOffset? DateMajUtc);