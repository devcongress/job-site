using FluentValidation;

namespace DevCongress.Jobs.Core.Features.Auth.Token.Register
{
  internal class TokenRegisterQueryValidator : AbstractValidator<TokenRegisterQuery>
  {
    public TokenRegisterQueryValidator()
    {
      RuleFor(request => request.Token).NotEmpty();
    }
  }
}
