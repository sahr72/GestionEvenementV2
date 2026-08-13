// EventManagement.Application/ReadModels/EventDetailsReadModel.cs

using EventManagement.Domain.SmartEnums;

namespace EventManagement.Application.ReadModels;

public sealed record EventDetailsReadModel
{
    public required Guid Id { get; init; }
    public required string Titre { get; init; }
    public required string Description { get; init; }
    public required string PersonneImpliquee { get; init; }
    public required DateTimeOffset DateCreationUtc { get; init; }
    public required DateTimeOffset? DateMiseAjourUtc { get; init; }
    public required EventStatus Statut { get; init; }
}