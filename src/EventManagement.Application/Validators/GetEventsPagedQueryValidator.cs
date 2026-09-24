// src/EventManagement.Application/Validators/GetEventsPagedQueryValidator.cs

namespace EventManagement.Application.Validators;

using EventManagement.Application.Queries.GetEventsPaged;
using FluentValidation;

public sealed class GetEventsPagedQueryValidator : AbstractValidator<GetEventsPagedQuery>
{
    public GetEventsPagedQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}