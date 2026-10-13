// src/EventManagement.Application/Validators/UpdateEventCommandValidator.cs

namespace EventManagement.Application.Validators;

using EventManagement.Application.Commands.UpdateEvent;
using FluentValidation;

/// <summary>
/// Valide les données entrantes d'une mise à jour d'événement.
/// Les règles sont identiques à la création, avec l'ajout de la validation de l'identifiant.
/// Les règles métier finales (statut, dates, propriétaire) restent dans le domaine.
/// </summary>
public sealed class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("L'identifiant de l'événement est requis.");

        RuleFor(x => x.Titre)
            .NotEmpty()
            .WithMessage("Le titre est requis.")
            .MaximumLength(100)
            .WithMessage("Le titre ne doit pas dépasser 100 caractères.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("La description est requise.");

        RuleFor(x => x.PersonneImpliquee)
            .NotEmpty()
            .WithMessage("La personne impliquée est requise.")
            .MaximumLength(120)
            .WithMessage("La personne impliquée ne doit pas dépasser 120 caractères.");
    }
}