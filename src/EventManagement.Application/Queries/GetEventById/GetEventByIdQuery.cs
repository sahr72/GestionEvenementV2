// EventManagement.Application/Queries/GetEventByIdQuery.cs

using EventManagement.Application.ReadModels;

namespace EventManagement.Application.Queries;

public sealed record GetEventByIdQuery(Guid Id);