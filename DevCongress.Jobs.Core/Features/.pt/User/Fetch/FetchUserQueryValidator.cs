using FluentValidation;

namespace DevCongress.Jobs.Core.Features.User.Fetch
{
    internal class FetchUserQueryValidator : AbstractValidator<FetchUserQuery>
    {
        public FetchUserQueryValidator()
        {
            RuleFor(request => request.Id).GreaterThan(0);
        }
    }
}