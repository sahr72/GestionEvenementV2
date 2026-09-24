// src/EventManagement.Application/Queries/GetComments/GetCommentsQuery.cs

namespace EventManagement.Application.Queries.GetComments;

public sealed record GetCommentsQuery(Guid EvenementId);