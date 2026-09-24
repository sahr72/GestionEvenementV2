// src/EventManagement.Application/DTOs/AddCommentRequest.cs

namespace EventManagement.Application.DTOs;

public sealed record AddCommentRequest(
    string Description,
    DateTimeOffset DateCreationUtc);