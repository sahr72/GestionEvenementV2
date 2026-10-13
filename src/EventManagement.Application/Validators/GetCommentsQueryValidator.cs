// src/EventManagement.Application/Validators/GetCommentsQueryValidator.cs

namespace EventManagement.Application.Validators;

using EventManagement.Application.Queries.GetComments;
using FluentValidation;

public sealed class GetCommentsQueryValidator : AbstractValidator<GetCommentsQuery>
{
    public GetCommentsQueryValidator()
    {
        RuleFor(x => x.EvenementId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'événement est requis.");
    }
}