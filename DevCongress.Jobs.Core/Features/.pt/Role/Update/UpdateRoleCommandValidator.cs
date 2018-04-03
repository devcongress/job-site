using FluentValidation;

namespace DevCongress.Jobs.Core.Features.Role.Update
{
    internal class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
    {
        public UpdateRoleCommandValidator()
        {
            RuleFor(request => request.Id).GreaterThan(0);
            RuleFor(request => request.Name).Length(fields => 3, fields => 255);
            RuleFor(request => request.Name).NotEmpty();
            RuleFor(request => request.Description).Length(fields => 0, fields => 255);
            RuleFor(request => request.Description).NotEmpty();
            RuleFor(request => request.Result).NotNull();
        }
    }
}