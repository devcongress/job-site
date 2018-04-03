using FluentValidation;

namespace DevCongress.Jobs.Core.Features.User.List
{
    internal class ListUsersQueryValidator : AbstractValidator<ListUsersQuery>
    {
        public ListUsersQueryValidator()
        {
            RuleFor(request => request.Page).GreaterThanOrEqualTo(1);
            RuleFor(request => request.Limit).GreaterThanOrEqualTo(0);
        }
    }
}