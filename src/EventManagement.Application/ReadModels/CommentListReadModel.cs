namespace EventManagement.Application.ReadModels;

public sealed record CommentListReadModel
{
    public required IReadOnlyList<CommentReadModel> Items { get; init; }
}