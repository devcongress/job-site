using FluentValidation;

namespace DevCongress.Jobs.Core.Features.User.SetUserPassword
{
    internal class SetUserPasswordCommandValidator : AbstractValidator<SetUserPasswordCommand>
    {
        public SetUserPasswordCommandValidator()
        {
            RuleFor(request => request.Id).GreaterThan(0);
            RuleFor(request => request.Password).Length(fields => 0, fields => 72);
            RuleFor(request => request.Password).NotEmpty();
            RuleFor(request => request.ConfirmPassword).Equal(fields => fields.Password);
            RuleFor(request => request.ConfirmPassword).NotEmpty();

            RuleFor(request => request.Result).NotNull();
        }
    }
}