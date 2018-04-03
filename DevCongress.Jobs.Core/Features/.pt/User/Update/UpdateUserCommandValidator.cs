using FluentValidation;

namespace DevCongress.Jobs.Core.Features.User.Update
{
    internal class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(request => request.Id).GreaterThan(0);
            RuleFor(request => request.Username).Length(fields => 0, fields => 50);
            RuleFor(request => request.Username).NotEmpty();
            RuleFor(request => request.Result).NotNull();
        }
    }
}