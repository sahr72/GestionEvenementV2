// src/EventManagement.Application/Commands/AddComment/AddCommentCommand.cs

namespace EventManagement.Application.Commands.AddComment;


//La commande porte l'identité de l'événement et l'identité de l'auteur du commentaire, mais pas l'identité du commentaire lui-même. L'identité du commentaire est générée par le domaine lors de la création du commentaire.
//'identité est injectée dans le command au niveau de l'endpoint, à partir du ClaimsPrincipal,
//via http.User.GetUserId(). pour CreateEvent et pour AddComment — deux cas où le command porte effectivement un userId, contrairement à UpdateEvenementCommand (l'endpoint ne connaît du propriétaire que ce que le filtre a vérifié).
//Note v3 : les rôles n'apparaissent pas ici. L'Application ne connaît ni Administrator, ni Organizer, ni Participant. Elle ne reçoit qu'un Guid userId et des permissions déjà résolues en policies.
public sealed record AddCommentCommand(
    Guid EventId,
    string Description,
    Guid AuteurId);