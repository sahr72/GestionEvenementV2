// src/EventManagement.Application/Validators/UpdateCommentCommandValidator.cs

namespace EventManagement.Application.Validators;

using EventManagement.Application.Commands.UpdateComment;
using FluentValidation;

public sealed class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.CommentId).NotEmpty();
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(1000);
    }
}