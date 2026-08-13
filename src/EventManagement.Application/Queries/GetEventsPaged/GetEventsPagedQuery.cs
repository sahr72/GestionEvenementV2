// EventManagement.Application/Queries/GetEventsPagedQuery.cs

using EventManagement.Application.ReadModels;

namespace EventManagement.Application.Queries;

public sealed record GetEventsPagedQuery(int Page,int PageSize);