// src/EventManagement.Application/Validators/CancelEventCommandValidator.cs

namespace EventManagement.Application.Validators;

using EventManagement.Application.Commands.CancelEvent;
using FluentValidation;

public sealed class CancelEventCommandValidator : AbstractValidator<CancelEventCommand>
{
    public CancelEventCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}