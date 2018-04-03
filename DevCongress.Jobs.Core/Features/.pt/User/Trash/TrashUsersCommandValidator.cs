using FluentValidation;

namespace DevCongress.Jobs.Core.Features.User.Trash
{
    internal class TrashUsersCommandValidator : AbstractValidator<TrashUsersCommand>
    {
        public TrashUsersCommandValidator()
        {
            RuleFor(request => request.Ids).NotEmpty();
            RuleFor(request => request.Result).NotNull();
        }
    }
}