// EventManagement.Application/Queries/GetEventByIdQuery.cs

using EventManagement.Application.ReadModels;

namespace EventManagement.Application.Queries.GetEventById;

public sealed record GetEventByIdQuery(Guid Id);