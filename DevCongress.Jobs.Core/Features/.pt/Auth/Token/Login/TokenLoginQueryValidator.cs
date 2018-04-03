using FluentValidation;

namespace DevCongress.Jobs.Core.Features.Auth.Token.Login
{
  internal class TokenLoginQueryValidator : AbstractValidator<TokenLoginQuery>
  {
    public TokenLoginQueryValidator()
    {
      RuleFor(request => request.Token).NotEmpty();
    }
  }
}
