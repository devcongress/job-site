using FluentValidation;

namespace DevCongress.Jobs.Core.Features.User.Restore
{
    internal class RestoreUsersCommandValidator : AbstractValidator<RestoreUsersCommand>
    {
        public RestoreUsersCommandValidator()
        {
            RuleFor(request => request.Ids).NotEmpty();
            RuleFor(request => request.Result).NotNull();
        }
    }
}