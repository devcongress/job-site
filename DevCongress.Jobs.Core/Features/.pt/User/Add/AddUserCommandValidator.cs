using FluentValidation;

namespace DevCongress.Jobs.Core.Features.User.Add
{
  internal class AddUserCommandValidator : AbstractValidator<AddUserCommand>
  {
    public AddUserCommandValidator()
    {
      RuleFor(request => request.TenantId).GreaterThan(0).When(request => request.TenantId != null);
      RuleFor(request => request.Username).Length(fields => 0, fields => 50);
      RuleFor(request => request.Username).NotEmpty();
      RuleFor(request => request.Password).Length(fields => 8, fields => 72);
      RuleFor(request => request.Password).NotEmpty();
      RuleFor(request => request.ConfirmPassword).Equal(fields => fields.Password);
      RuleFor(request => request.ConfirmPassword).NotEmpty();
      RuleFor(request => request.Result).NotNull();
    }
  }
}
