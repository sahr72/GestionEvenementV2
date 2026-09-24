using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Application.Validators
{
    using EventManagement.Application.Commands.DeleteComment;
    using FluentValidation;

    public sealed class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
    {
        public DeleteCommentCommandValidator()
        {
            RuleFor(x => x.EventId).NotEmpty();
            RuleFor(x => x.CommentId).NotEmpty();
        }
    }
}
