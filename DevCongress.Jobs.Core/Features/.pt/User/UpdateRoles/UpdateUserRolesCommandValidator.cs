using FluentValidation;

namespace DevCongress.Jobs.Core.Features.User.UpdateRoles
{
    internal class UpdateUserRolesCommandValidator : AbstractValidator<UpdateUserRolesCommand>
    {
        public UpdateUserRolesCommandValidator()
        {
            RuleFor(request => request.Id).GreaterThan(0);
            RuleFor(request => request.RoleIds).NotNull();
            RuleFor(request => request.Result).NotNull();
        }
    }
}