using FluentValidation;

namespace DevCongress.Jobs.Core.Features.Role.UpdatePermissions
{
    internal class UpdateRolePermissionsCommandValidator : AbstractValidator<UpdateRolePermissionsCommand>
    {
        public UpdateRolePermissionsCommandValidator()
        {
            RuleFor(request => request.Id).GreaterThan(0);
            RuleFor(request => request.Permissions).NotNull();
            RuleFor(request => request.Result).NotNull();
        }
    }
}