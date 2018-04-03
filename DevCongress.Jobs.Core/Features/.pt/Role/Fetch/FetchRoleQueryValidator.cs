using FluentValidation;

namespace DevCongress.Jobs.Core.Features.Role.Fetch
{
    internal class FetchRoleQueryValidator : AbstractValidator<FetchRoleQuery>
    {
        public FetchRoleQueryValidator()
        {
            RuleFor(request => request.Id).GreaterThan(0);
        }
    }
}