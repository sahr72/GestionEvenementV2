// src/EventManagement.Application/Validators/CloseEventCommandValidator.cs

namespace EventManagement.Application.Validators;

using EventManagement.Application.Commands.CloseEvent;
using FluentValidation;

public sealed class CloseEventCommandValidator : AbstractValidator<CloseEventCommand>
{
    public CloseEventCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}