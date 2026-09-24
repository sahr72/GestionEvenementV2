// src/EventManagement.Application/Validators/CreateEventCommandValidator.cs

namespace EventManagement.Application.Validators;

using EventManagement.Application.Commands.CreateEvent;
using FluentValidation;

public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Titre)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty();

        RuleFor(x => x.PersonneImpliquee)
            .NotEmpty()
            .MaximumLength(120);
    }
}