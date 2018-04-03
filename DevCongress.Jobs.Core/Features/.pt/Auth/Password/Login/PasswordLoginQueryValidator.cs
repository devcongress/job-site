using FluentValidation;

namespace DevCongress.Jobs.Core.Features.Auth.Password.Login
{
  internal class PasswordLoginQueryValidator : AbstractValidator<PasswordLoginQuery>
  {
    public PasswordLoginQueryValidator()
    {
      RuleFor(request => request.Username).NotEmpty();
      RuleFor(request => request.Password).NotEmpty();
    }
  }
}
