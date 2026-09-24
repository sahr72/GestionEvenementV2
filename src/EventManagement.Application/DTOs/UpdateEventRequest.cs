// src/EventManagement.Application/DTOs/UpdateEventRequest.cs
namespace EventManagement.Application.DTOs;

public sealed record UpdateEventRequest(
    string Titre,
    string Description,
    string PersonneImpliquee);
/*
 * L'identifiant ( Id) est fourni via la route dans l'API ( /api/events/{id}), il n'est pas donc pas inclus dans le DTO.
 */