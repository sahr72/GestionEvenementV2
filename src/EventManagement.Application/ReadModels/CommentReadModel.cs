namespace EventManagement.Application.ReadModels;
public sealed record CommentReadModel
{
    public required Guid Id { get; init; }
    public required string Description { get; init; }
    public required DateTime Date { get; init; }
    public required Guid EventId { get; init; }
}