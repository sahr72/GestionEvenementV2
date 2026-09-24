// src/EventManagement.Application/Validators/ConfirmEventCommandValidator.cs

namespace EventManagement.Application.Validators;

using EventManagement.Application.Commands.ConfirmEvent;
using FluentValidation;

public sealed class ConfirmEventCommandValidator : AbstractValidator<ConfirmEventCommand>
{
    public ConfirmEventCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}