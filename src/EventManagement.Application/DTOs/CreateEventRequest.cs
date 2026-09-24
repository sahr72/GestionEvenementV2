// src/EventManagement.Application/DTOs/CreateEventRequest.cs

namespace EventManagement.Application.DTOs;

public sealed record CreateEventRequest(
    string Titre,
    string Description,
    string PersonneImpliquee);