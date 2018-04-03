using FluentValidation;

namespace DevCongress.Jobs.Core.Features.Role.List
{
    internal class ListRolesQueryValidator : AbstractValidator<ListRolesQuery>
    {
        public ListRolesQueryValidator()
        {
            RuleFor(request => request.Page).GreaterThanOrEqualTo(1);
            RuleFor(request => request.Limit).GreaterThanOrEqualTo(0);
        }
    }
}