using FluentValidation;

namespace DevCongress.Jobs.Core.Features.Role.Trash
{
    internal class TrashRolesCommandValidator : AbstractValidator<TrashRolesCommand>
    {
        public TrashRolesCommandValidator()
        {
            RuleFor(request => request.Ids).NotEmpty();
            RuleFor(request => request.Result).NotNull();
        }
    }
}