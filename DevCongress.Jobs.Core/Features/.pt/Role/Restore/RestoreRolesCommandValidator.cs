using FluentValidation;

namespace DevCongress.Jobs.Core.Features.Role.Restore
{
    internal class RestoreRolesCommandValidator : AbstractValidator<RestoreRolesCommand>
    {
        public RestoreRolesCommandValidator()
        {
            RuleFor(request => request.Ids).NotEmpty();
            RuleFor(request => request.Result).NotNull();
        }
    }
}