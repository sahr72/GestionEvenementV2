// EventManagement.Application/Queries/GetEventsPagedQuery.cs

using EventManagement.Application.ReadModels;
namespace EventManagement.Application.Queries.GetEventsPaged;
//namespace EventManagement.Application.Queries;

public sealed record GetEventsPagedQuery(int Page,int PageSize,string? Statut);
// src/EventManagement.Application/Queries/GetEventsPaged/GetEventsPagedQuery.cs



//public sealed record GetEventsPagedQuery(
//    int Page,
//    int PageSize,
//    string? Status);