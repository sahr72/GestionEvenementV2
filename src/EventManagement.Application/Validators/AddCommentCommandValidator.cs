// src/EventManagement.Application/Validators/UpdateCommentCommandValidator.cs
// src/EventManagement.Application/Validators/AddCommentCommandValidator.cs

namespace EventManagement.Application.Validators;

using EventManagement.Application.Commands.AddComment;
using FluentValidation;

public sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'événement est requis.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("La description du commentaire est requise.")
            .MaximumLength(2000)
            .WithMessage("La description ne doit pas dépasser 2000 caractères.");

        RuleFor(x => x.AuteurId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'utilisateur est requis.");
    }
}